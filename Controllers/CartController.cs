using ElectronicsStore.Data;
using ElectronicsStore.Models;
using ElectronicsStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ElectronicsStore.Controllers
{
    /*
     * CartController handles all shopping cart functionality
     * This controller manages adding products to cart, updating quantities,
     * removing items, displaying cart contents, and clearing the cart
     * It supports both regular page loads and AJAX requests for a smoother user experience
     */
    public class CartController : Controller
    {
        // CartService handles the business logic for cart operations
        private readonly CartService _cartService;
        
        // Database context for accessing application data directly when needed
        private readonly ApplicationDbContext _context;

        // Constructor: Initializes services via dependency injection
        public CartController(CartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        // Displays the shopping cart page with all items and total
        // GET: /Cart
        // Shows all items in the user's cart along with quantities and prices
        public async Task<IActionResult> Index()
        {
            // Get all items in the current user's cart
            var cartItems = await _cartService.GetCartItemsAsync();
            
            // Calculate the total price of all items in the cart
            var cartTotal = await _cartService.GetCartTotalAsync();

            // Pass the total to the view via ViewData
            ViewData["CartTotal"] = cartTotal;
            
            // Return the cart view with the list of cart items
            return View(cartItems);
        }

        // Adds a product to the shopping cart
        // POST: /Cart/AddToCart
        // Parameters: productId - The ID of the product to add
        //             quantity - The quantity to add (defaults to 1)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Ensure quantity is valid (minimum 1)
                if (quantity <= 0)
                {
                    quantity = 1;
                }

                // Add the product to the cart using the cart service
                await _cartService.AddToCartAsync(productId, quantity);
                
                // For AJAX requests, return a JSON result with updated cart count
                // This enables updating the cart icon in the navbar without page refresh
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cartCount = await _cartService.GetCartItemCountAsync();
                    return Json(new { success = true, message = "Product added to cart!", cartCount });
                }

                // For regular requests, set a success message and redirect to cart page
                TempData["SuccessMessage"] = "Product added to cart!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Handle errors differently for AJAX vs regular requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // Updates the quantity of an item in the cart
        // POST: /Cart/UpdateQuantity
        // Parameters: cartItemId - The ID of the cart item to update
        //             quantity - The new quantity value
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                // Update the cart item quantity using the cart service
                await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                
                // For AJAX requests, return updated cart details for dynamic UI updates
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cartItems = await _cartService.GetCartItemsAsync();
                    var cartTotal = await _cartService.GetCartTotalAsync();
                    var cartCount = await _cartService.GetCartItemCountAsync();
                    
                    // Calculate the subtotal for this specific item
                    return Json(new 
                    { 
                        success = true, 
                        cartTotal, 
                        cartCount,
                        itemTotal = cartItems.FirstOrDefault(i => i.Id == cartItemId)?.Product.Price * quantity 
                    });
                }

                // For regular requests, redirect back to the cart page
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Handle errors differently for AJAX vs regular requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Removes an item from the shopping cart
        // POST: /Cart/RemoveItem
        // Parameters: cartItemId - The ID of the cart item to remove
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            // Remove the item from the cart using the cart service
            await _cartService.RemoveFromCartAsync(cartItemId);
            
            // For AJAX requests, return updated cart totals for dynamic UI updates
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cartTotal = await _cartService.GetCartTotalAsync();
                var cartCount = await _cartService.GetCartItemCountAsync();
                return Json(new { success = true, cartTotal, cartCount });
            }

            // For regular requests, redirect back to the cart page
            return RedirectToAction("Index");
        }

        // Removes all items from the shopping cart
        // POST: /Cart/Clear
        // Completely empties the user's cart
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            // Clear all items from the cart using the cart service
            await _cartService.ClearCartAsync();
            
            // For AJAX requests, return a simple success response
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            // For regular requests, redirect back to the cart page (which will now be empty)
            return RedirectToAction("Index");
        }
    }
} 