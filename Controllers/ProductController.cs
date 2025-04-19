using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Controllers
{
    /*
     * ProductController handles all product management functionality
     * This controller is restricted to users without the Admin role
     * It provides CRUD operations for products, including image upload handling
     * and category selection
     */
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        // Database context for accessing application data
        private readonly ApplicationDbContext _context;
        
        // Web host environment for file system operations (image uploads)
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor: Initializes services via dependency injection
        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Displays a list of all products for admin management
        // GET: Products
        // Shows all products with their category information in a tabular format
        public async Task<IActionResult> Index()
        {
            // Retrieve all products with their category information
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
                
            // Pass the products to the view
            return View(products);
        }

        // Displays detailed information about a specific product
        // GET: Products/Details/5
        // Parameters: id - The ID of the product to display
        public async Task<IActionResult> Details(int? id)
        {
            // Check if the ID parameter is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the product with its category information
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            // Return 404 if the product doesn't exist
            if (product == null)
            {
                return NotFound();
            }

            // Display the product details view
            return View(product);
        }

        // Displays the form to create a new product
        // GET: Products/Create
        // Shows form with category dropdown list
        public IActionResult Create()
        {
            // Prepare the categories dropdown list for the form
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            
            // Display the empty product creation form
            return View();
        }

        // Processes the form submission to create a new product
        // POST: Products/Create
        // Parameters: product - The product data from the form
        //             imageFile - The uploaded product image file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,CategoryId,StockQuantity,IsAvailable")] Product product, IFormFile? imageFile)
        {
            // Check if the model data is valid
            if (ModelState.IsValid)
            {
                // Handle image upload if provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Save the uploaded image and get its path
                    product.ImageUrl = await SaveImage(imageFile);
                }
                else
                {
                    // Set default image path if no image was uploaded
                    product.ImageUrl = "/images/products/default-product.jpg";
                }

                // Add the product to the database
                _context.Add(product);
                await _context.SaveChangesAsync();
                
                // Set success message and redirect to product list
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            // If model validation fails, repopulate the categories dropdown
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            
            // Return to the form with validation errors
            return View(product);
        }

        // Displays the form to edit an existing product
        // GET: Products/Edit/5
        // Parameters: id - The ID of the product to edit
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if the ID parameter is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the product to edit
            var product = await _context.Products.FindAsync(id);
            
            // Return 404 if the product doesn't exist
            if (product == null)
            {
                return NotFound();
            }
            
            // Prepare the categories dropdown, pre-selecting the product's current category
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            
            // Display the edit form with the product data
            return View(product);
        }

        // Processes the form submission to update an existing product
        // POST: Products/Edit/5
        // Parameters: id - The ID of the product to update
        //             product - The updated product data from the form
        //             imageFile - The uploaded product image file (optional)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId,StockQuantity,IsAvailable,ImageUrl")] Product product, IFormFile? imageFile)
        {
            // Check if the ID in the URL matches the ID in the form data
            if (id != product.Id)
            {
                return NotFound();
            }

            // Check if the model data is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload if a new image was provided
                    // If no new image, keep the existing ImageUrl from form
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Save the new image and update the path
                        product.ImageUrl = await SaveImage(imageFile);
                    }

                    // Update the product in the database
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    
                    // Set success message
                    TempData["SuccessMessage"] = "Product updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                // Redirect to the product list
                return RedirectToAction(nameof(Index));
            }
            
            // If model validation fails, repopulate the categories dropdown
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            
            // Return to the form with validation errors
            return View(product);
        }

        // Displays the confirmation page for deleting a product
        // GET: Products/Delete/5
        // Parameters: id - The ID of the product to delete
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if the ID parameter is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the product with its category information
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            // Return 404 if the product doesn't exist
            if (product == null)
            {
                return NotFound();
            }

            // Display the delete confirmation page
            return View(product);
        }

        // Processes the confirmation to delete a product
        // POST: Products/Delete/5
        // Parameters: id - The ID of the product to delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the product to delete
            var product = await _context.Products.FindAsync(id);
            
            // Return 404 if the product doesn't exist
            if (product == null)
            {
                return NotFound();
            }

            // Remove the product from the database
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            // Set success message and redirect to product list
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a product exists by ID
        // Used internally by the controller for validation
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Helper method to save uploaded image files
        // Returns the relative path to the saved image for storing in the database
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string uniqueFileName = null;
            
            if (imageFile != null)
            {
                // Define the upload directory
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                
                // Create directory if it doesn't exist
                // This ensures the upload won't fail if the folder structure isn't present
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                // Generate a unique filename to prevent overwriting existing files
                // Combines a GUID with the original filename to ensure uniqueness
                uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                // Save the file to the server's file system
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                
                // Return the relative path for storage in the database
                // Using a relative path makes it website-agnostic
                return "/images/products/" + uniqueFileName;
            }
            
            // Return null if no file was provided
            return uniqueFileName;
        }
    }
} 