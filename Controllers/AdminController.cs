using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Controllers
{
    /*
     * AdminController handles all admin dashboard functionality
     * This controller is completely restricted to users without the Admin role
     * It provides overview statistics and user management capabilities
     */
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Database context for accessing application data
        private readonly ApplicationDbContext _context;
        
        // UserManager for accessing and managing user accounts
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor: Initializes services via dependency injection
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the admin dashboard with overview statistics
        // GET: /Admin/Index
        // Shows counts of products, categories, users, and orders
        public async Task<IActionResult> Index()
        {
            // Count total products in the database
            ViewBag.ProductCount = await _context.Products.CountAsync();
            
            // Count total categories in the database
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            
            // Count total registered users
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            
            // Count total orders placed in the system
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            
            // Render the admin dashboard view with these statistics
            return View();
        }

        // Displays a list of all registered users
        // GET: /Admin/Users
        // Used by administrators to view and potentially manage user accounts
        public async Task<IActionResult> Users()
        {
            // Retrieve all users from the database
            var users = await _userManager.Users.ToListAsync();
            
            // Pass the list of users to the view for display
            return View(users);
        }
    }
} 