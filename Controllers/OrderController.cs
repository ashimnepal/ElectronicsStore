using ElectronicsStore.Data;
using ElectronicsStore.Models;
using ElectronicsStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElectronicsStore.Controllers
{
    /*
     * OrderController handles all order-related functionality
     * This controller manages the complete order process including:
     * - Viewing order history for customers
     * - Checkout process (standard, quick, and direct checkout options)
     * - Order creation and cart clearing
     * - Admin-only order management features
     */
    public class OrderController : Controller
    {
        // Database context for accessing application data
        private readonly ApplicationDbContext _context;
        
        // UserManager for accessing user information
        private readonly UserManager<ApplicationUser> _userManager;
        
        // CartService for managing shopping cart operations
        private readonly CartService _cartService;

        // Constructor: Initializes services via dependency injection
        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        // Displays a list of orders for the current user
        // GET: /Order
        // Requires the user to be authenticated
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get current user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Retrieve all orders for this user, ordered by date (newest first)
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Pass the list of orders to the My Orders view
            return View(orders);
        }

        // Displays detailed information about a specific order
        // GET: /Order/Details/{id}
        // Parameters: id - The ID of the order to display
        // Requires the user to be authenticated
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            // Get current user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Find the order with its related items and products
            // Also filter by userId to ensure users can only see their own orders
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            // Return 404 if the order doesn't exist or doesn't belong to the current user
            if (order == null)
            {
                return NotFound();
            }

            // Display the order details view with the complete order information
            return View(order);
        }

        // Displays detailed information about a specific order for admins
        // GET: /Order/AdminOrderDetails/{id}
        // Parameters: id - The ID of the order to display
        // Only accessible to users with the Admin role
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrderDetails(int id)
        {
            // Find the order with its related items and products
            // No user ID filter for admins - they can view any order
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            // Return 404 if the order doesn't exist
            if (order == null)
            {
                return NotFound();
            }

            // Display the order details view with the complete order information
            return View("Details", order);
        }

        // Displays the checkout form with shipping and payment options
        // GET: /Order/Checkout
        // Requires the user to be authenticated
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            // Get all items in the user's cart
            var cartItems = await _cartService.GetCartItemsAsync();

            // Redirect to cart if it's empty
            if (cartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            // Calculate the total amount for all items in the cart
            var total = cartItems.Sum(item => item.Product.Price * item.Quantity);

            // Pass the total to the view via ViewBag
            ViewBag.Total = total;
            
            // Display the checkout form
            return View();
        }

        // Processes the quick checkout option (skips the checkout form)
        // POST: /Order/QuickCheckout
        // Creates an order with default shipping and payment values
        // Requires the user to be authenticated
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> QuickCheckout()
        {
            // Get current user ID and user details
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            
            // Get all items in the user's cart
            var cartItems = await _cartService.GetCartItemsAsync();

            // Redirect to cart if it's empty
            if (cartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            // Calculate the total amount for all items in the cart
            var total = cartItems.Sum(item => item.Product.Price * item.Quantity);

            // Create the order with default values
            // This is the "quick checkout" option that uses pre-defined values
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                ShippingAddress = user?.Address ?? "Default shipping address", // Use user's address or default
                PaymentMethod = "Cash on Delivery", // Default payment method
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            // Add all cart items to the order
            foreach (var item in cartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price // Store current price at time of purchase
                });
            }

            // Save the order to the database
            _context.Orders.Add(order);

            // Clear the cart after successful order creation
            await _cartService.ClearCartAsync();
            await _context.SaveChangesAsync();

            // Set success message and redirect to order history
            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Processes the standard checkout form submission
        // POST: /Order/PlaceOrder
        // Parameters: shippingAddress - The delivery address
        //             paymentMethod - The selected payment method
        // Requires the user to be authenticated
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string paymentMethod)
        {
            try 
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Get all items in the user's cart
                var cartItems = await _cartService.GetCartItemsAsync();

                // Redirect to cart if it's empty
                if (cartItems.Count == 0)
                {
                    TempData["Error"] = "Your cart is empty";
                    return RedirectToAction("Index", "Cart");
                }

                // Calculate the total amount for all items in the cart
                var total = cartItems.Sum(item => item.Product.Price * item.Quantity);

                // Create the order with values from the form
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = total,
                    ShippingAddress = shippingAddress,
                    PaymentMethod = paymentMethod,
                    Status = OrderStatus.Pending,
                    OrderItems = new List<OrderItem>()
                };

                // Add all cart items to the order
                foreach (var item in cartItems)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price // Store current price at time of purchase
                    });
                }

                // Save the order to the database
                _context.Orders.Add(order);

                // Clear the cart after successful order creation
                await _cartService.ClearCartAsync();
                await _context.SaveChangesAsync();

                // Set success message and redirect to order history
                TempData["Success"] = "Your order has been placed successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in PlaceOrder: {ex.Message}");
                
                // Set error message and redirect back to cart
                TempData["Error"] = "There was an error placing your order. Please try again.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // Processes the direct checkout from the cart modal
        // POST: /Order/DirectCheckout
        // Parameters: shippingAddress - The delivery address
        //             paymentMethod - The selected payment method
        // Requires the user to be authenticated
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DirectCheckout(string shippingAddress, string paymentMethod)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Get all items in the user's cart
                var cartItems = await _cartService.GetCartItemsAsync();

                // Redirect to cart if it's empty
                if (cartItems.Count == 0)
                {
                    TempData["Error"] = "Your cart is empty";
                    return RedirectToAction("Index", "Cart");
                }

                // Calculate the total amount for all items in the cart
                var total = cartItems.Sum(item => item.Product.Price * item.Quantity);

                // Create the order with values from the modal form
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = total,
                    ShippingAddress = shippingAddress,
                    PaymentMethod = paymentMethod,
                    Status = OrderStatus.Pending,
                    OrderItems = new List<OrderItem>()
                };

                // Add all cart items to the order
                foreach (var item in cartItems)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price // Store current price at time of purchase
                    });
                }

                // Save the order to the database
                _context.Orders.Add(order);

                // Clear the cart after successful order creation
                await _cartService.ClearCartAsync();
                await _context.SaveChangesAsync();

                // Set success message and redirect to order history
                TempData["Success"] = "Your order has been placed successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in DirectCheckout: {ex.Message}");
                
                // Set error message and redirect back to cart
                TempData["Error"] = "There was an error placing your order. Please try again.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // Displays all orders for administrative management
        // GET: /Order/ManageOrders
        // Only accessible to users with the Admin role
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageOrders()
        {
            // Retrieve all orders with user information and order items
            // This gives admins a complete view of all orders in the system
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Display the admin order management view
            return View(orders);
        }

        // Updates the status of an order (admin function)
        // GET: /Order/UpdateStatus/{id}?status={status}
        // Parameters: id - The ID of the order to update
        //             status - The new status value
        // Only accessible to users with the Admin role
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            // Find the order to update
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Update the order status
            order.Status = status;
            await _context.SaveChangesAsync();

            // Set success message and redirect back to order management
            TempData["Success"] = "Order status updated successfully";
            return RedirectToAction(nameof(ManageOrders));
        }

        // Retrieves order items for display in the admin panel via AJAX
        // GET: /Order/GetOrderItems/{id}
        // Parameters: id - The ID of the order to get items for
        // Only accessible to users with the Admin role
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderItems(int id)
        {
            try
            {
                // Find the order with its related items and products
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                // Return 404 if the order doesn't exist
                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }

                // Check if order has any items
                if (order.OrderItems == null || !order.OrderItems.Any())
                {
                    return PartialView("_OrderItemsPartial", new List<OrderItem>());
                }

                // Return a partial view with just the order items
                // This is used for AJAX loading in the admin order details modal
                return PartialView("_OrderItemsPartial", order.OrderItems);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in GetOrderItems: {ex.Message}");
                
                // Return an error message
                return StatusCode(500, $"An error occurred while retrieving order items: {ex.Message}");
            }
        }
    }
} 