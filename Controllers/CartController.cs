using ElectronicsStore.Data;
using ElectronicsStore.Models;
using ElectronicsStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ElectronicsStore.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;

        public CartController(CartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            var cartTotal = await _cartService.GetCartTotalAsync();

            ViewData["CartTotal"] = cartTotal;
            return View(cartItems);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    quantity = 1;
                }

                await _cartService.AddToCartAsync(productId, quantity);
                
                // For AJAX requests, return a JSON result
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cartCount = await _cartService.GetCartItemCountAsync();
                    return Json(new { success = true, message = "Product added to cart!", cartCount });
                }

                TempData["SuccessMessage"] = "Product added to cart!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cartItems = await _cartService.GetCartItemsAsync();
                    var cartTotal = await _cartService.GetCartTotalAsync();
                    var cartCount = await _cartService.GetCartItemCountAsync();
                    
                    return Json(new 
                    { 
                        success = true, 
                        cartTotal, 
                        cartCount,
                        itemTotal = cartItems.FirstOrDefault(i => i.Id == cartItemId)?.Product.Price * quantity 
                    });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cartTotal = await _cartService.GetCartTotalAsync();
                var cartCount = await _cartService.GetCartItemCountAsync();
                return Json(new { success = true, cartTotal, cartCount });
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearCartAsync();
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }
    }
} 