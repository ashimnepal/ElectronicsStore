using ElectronicsStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartSummaryViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartItemCount = await _cartService.GetCartItemCountAsync();
            return View(cartItemCount);
        }
    }
} 