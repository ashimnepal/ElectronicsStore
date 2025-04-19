using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Models;
using ElectronicsStore.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Controllers;

/*
 * HomeController handles the main public-facing pages of the website
 * This controller provides the homepage, product details, category browsing,
 * and other general pages accessible to all users without authentication
 * It serves as the primary entry point for customers browsing the store
 */
public class HomeController : Controller
{
    // Logger for recording diagnostic information and errors
    private readonly ILogger<HomeController> _logger;
    
    // Database context for accessing application data
    private readonly ApplicationDbContext _context;

    // Constructor: Initializes services via dependency injection
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Displays the main homepage of the website
    // GET: Home/Index
    // Shows featured products, category cards, and promotional content
    public async Task<IActionResult> Index()
    {
        // Get featured products (showing the latest 8 products that are in stock)
        // This query finds available products with stock, includes their category information,
        // orders them by ID (descending to get newest first) and takes only 8 items
        var featuredProducts = await _context.Products
            .Where(p => p.IsAvailable && p.StockQuantity > 0)
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .Take(8)
            .ToListAsync();

        // Get all categories to ensure we can display exactly 6 category cards on the homepage
        // These are passed to the view through ViewBag to keep the main model focused on products
        var allCategories = await _context.Categories.ToListAsync();
        ViewBag.AllCategories = allCategories;

        // Return the homepage view with featured products as the model
        return View(featuredProducts);
    }

    // Displays detailed information about a specific product
    // GET: Home/ProductDetails/{id}
    // Parameters: id - The ID of the product to display
    // Shows full product details, images, pricing, and related products
    public async Task<IActionResult> ProductDetails(int id)
    {
        // Find the requested product with its category information
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Return 404 if the product doesn't exist
        if (product == null)
        {
            return NotFound();
        }

        // Get related products from the same category
        // This helps users discover similar products they might be interested in
        // Excludes the current product and only includes available products
        var relatedProducts = await _context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsAvailable)
            .Take(4)
            .ToListAsync();

        // Pass related products to the view via ViewData
        ViewData["RelatedProducts"] = relatedProducts;

        // Return the product details view with the product as the model
        return View(product);
    }

    // Displays all products in a specific category
    // GET: Home/CategoryProducts/{categoryId}
    // Parameters: categoryId - The ID of the category to browse
    // Shows a filtered list of products from the selected category
    public async Task<IActionResult> CategoryProducts(int categoryId)
    {
        // Find the requested category
        var category = await _context.Categories.FindAsync(categoryId);
        
        // Return 404 if the category doesn't exist
        if (category == null)
        {
            return NotFound();
        }

        // Get all products in this category
        // Includes category information for each product
        var products = await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .ToListAsync();

        // Pass category information to the view via ViewData
        // This allows the view to display the category name as a header
        ViewData["CategoryName"] = category.Name;
        ViewData["CategoryId"] = categoryId;

        // Return the category products view with the list of products as the model
        return View(products);
    }

    // Displays the privacy policy page
    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // Displays the error page when exceptions occur
    // GET: Home/Error
    // This method has caching disabled to ensure fresh error information
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Create an error view model with the request ID for tracking
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
