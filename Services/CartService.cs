using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Get or create a cart ID for the current session
        public string GetCartId()
        {
            // If user is authenticated, use their user ID as the cart ID
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return _httpContextAccessor.HttpContext.User.Identity.Name;
            }

            // For anonymous users, use session-based cart ID
            var cartId = _httpContextAccessor.HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                _httpContextAccessor.HttpContext.Session.SetString("CartId", cartId);
            }

            return cartId;
        }

        // Add an item to the cart
        public async Task AddToCartAsync(int productId, int quantity)
        {
            var cartId = GetCartId();

            // Check if the product exists and is available
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable || product.StockQuantity <= 0)
            {
                throw new InvalidOperationException("Product is not available.");
            }

            // Check if we already have this item in the cart
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);

            if (cartItem != null)
            {
                // Item exists in cart, update quantity
                cartItem.Quantity += quantity;
            }
            else
            {
                // Add new cart item
                cartItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity,
                    DateCreated = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        // Get all items in the cart
        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            var cartId = GetCartId();
            return await _context.CartItems
                .Where(c => c.CartId == cartId)
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();
        }

        // Update cart item quantity
        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartId = GetCartId();
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.CartId == cartId);

            if (cartItem == null)
            {
                throw new InvalidOperationException("Cart item not found.");
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
        }

        // Remove an item from the cart
        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartId = GetCartId();
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.CartId == cartId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        // Clear the cart
        public async Task ClearCartAsync()
        {
            var cartId = GetCartId();
            var cartItems = await _context.CartItems
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        // Calculate the total price of items in the cart
        public async Task<decimal> GetCartTotalAsync()
        {
            var cartItems = await GetCartItemsAsync();
            return cartItems.Sum(i => i.Product.Price * i.Quantity);
        }

        // Get the number of items in the cart
        public async Task<int> GetCartItemCountAsync()
        {
            var cartItems = await GetCartItemsAsync();
            return cartItems.Sum(i => i.Quantity);
        }
    }
} 