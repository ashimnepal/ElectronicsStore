using ElectronicsStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.ViewComponents
{
    /*
     * CartSummaryViewComponent is a reusable UI component that displays
     * the current cart item count, typically in the navigation bar
     * View components are similar to partial views but with their own logic
     */
    public class CartSummaryViewComponent : ViewComponent
    {
        // CartService dependency for accessing shopping cart data
        private readonly CartService _cartService;

        // Constructor: Initializes the CartService dependency via injection
        public CartSummaryViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        // The main method called when the view component is invoked in a view
        // Returns the number of items in the current user's shopping cart
        // This is rendered in the default view for this component
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get the current cart item count from the cart service
            // This works for both authenticated and unauthenticated users
            var cartItemCount = await _cartService.GetCartItemCountAsync();
            
            // Return the default view with the cart item count as the model
            // This will be rendered in ~/Views/Shared/Components/CartSummary/Default.cshtml
            return View(cartItemCount);
        }
    }
} 