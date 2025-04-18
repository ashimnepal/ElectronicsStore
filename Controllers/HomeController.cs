using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Models;
using ElectronicsStore.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get featured products (showing the latest 8 products)
        var featuredProducts = await _context.Products
            .Where(p => p.IsAvailable && p.StockQuantity > 0)
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .Take(8)
            .ToListAsync();

        return View(featuredProducts);
    }

    public async Task<IActionResult> ProductDetails(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // Get related products from the same category
        var relatedProducts = await _context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsAvailable)
            .Take(4)
            .ToListAsync();

        ViewData["RelatedProducts"] = relatedProducts;

        return View(product);
    }

    public async Task<IActionResult> CategoryProducts(int categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return NotFound();
        }

        var products = await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .ToListAsync();

        ViewData["CategoryName"] = category.Name;
        ViewData["CategoryId"] = categoryId;

        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
