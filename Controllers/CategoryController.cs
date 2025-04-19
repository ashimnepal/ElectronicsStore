using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Controllers
{
    /*
     * CategoryController handles all category management functionality
     * This controller is restricted to users with the Admin role
     * It provides CRUD operations for product categories, including validation
     * to prevent deletion of categories that contain products
     */
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        // Database context for accessing application data
        private readonly ApplicationDbContext _context;

        // Constructor: Initializes the database context via dependency injection
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays a list of all product categories
        // GET: Categories
        // Shows all categories in a tabular format for admin management
        public async Task<IActionResult> Index()
        {
            // Retrieve all categories from the database and pass to the view
            return View(await _context.Categories.ToListAsync());
        }

        // Displays detailed information about a specific category
        // GET: Categories/Details/5
        // Parameters: id - The ID of the category to display
        public async Task<IActionResult> Details(int? id)
        {
            // Check if the ID parameter is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the category with the specified ID
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
                
            // Return 404 if the category doesn't exist
            if (category == null)
            {
                return NotFound();
            }

            // Display the category details
            return View(category);
        }

        // Displays the form to create a new category
        // GET: Categories/Create
        public IActionResult Create()
        {
            // Return the empty form view for creating a new category
            return View();
        }

        // Processes the form submission to create a new category
        // POST: Categories/Create
        // Parameters: category - The category data from the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            // Check if the submitted data is valid according to model validation rules
            if (ModelState.IsValid)
            {
                // Add the new category to the database
                _context.Add(category);
                await _context.SaveChangesAsync();
                
                // Set a success message that will be displayed on the next page
                TempData["SuccessMessage"] = "Category created successfully.";
                
                // Redirect to the index page to show all categories, including the new one
                return RedirectToAction(nameof(Index));
            }
            
            // If model validation fails, return to the form with validation errors
            return View(category);
        }

        // Displays the form to edit an existing category
        // GET: Categories/Edit/5
        // Parameters: id - The ID of the category to edit
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if the ID parameter is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the category to edit
            var category = await _context.Categories.FindAsync(id);
            
            // Return 404 if the category doesn't exist
            if (category == null)
            {
                return NotFound();
            }
            
            // Display the edit form with the category data
            return View(category);
        }

        // Processes the form submission to update an existing category
        // POST: Categories/Edit/5
        // Parameters: id - The ID of the category to update
        //             category - The updated category data from the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            // Check if the ID in the URL matches the ID in the form data
            if (id != category.Id)
            {
                return NotFound();
            }

            // Check if the submitted data is valid according to model validation rules
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the category in the database
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    
                    // Set a success message that will be displayed on the next page
                    TempData["SuccessMessage"] = "Category updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts (when the record was updated by another user)
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                // Redirect to the index page to show all categories
                return RedirectToAction(nameof(Index));
            }
            
            // If model validation fails, return to the form with validation errors
            return View(category);
        }

        // Displays the confirmation page for deleting a category
        // GET: Categories/Delete/5
        // Parameters: id - The ID of the category to delete
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if the ID parameter is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the category to delete
            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
                
            // Return 404 if the category doesn't exist
            if (category == null)
            {
                return NotFound();
            }

            // Display the delete confirmation page
            return View(category);
        }

        // Processes the confirmation to delete a category
        // POST: Categories/Delete/5
        // Parameters: id - The ID of the category to delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the category to delete
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if any products are using this category
            // This prevents orphaned products without a valid category
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                // If the category has products, prevent deletion and show an error message
                TempData["ErrorMessage"] = "Cannot delete category because it contains products.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Delete the category from the database
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            
            // Set a success message that will be displayed on the next page
            TempData["SuccessMessage"] = "Category deleted successfully.";
            
            // Redirect to the index page to show the updated list of categories
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a category exists by ID
        // Used internally by the controller for validation
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
} 