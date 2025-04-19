using ElectronicsStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Data
{
    /*
     * SeedData provides functionality to populate the database with initial data
     * This static class contains methods for creating default roles, admin user,
     * and product categories when the application is first deployed
     */
    public static class SeedData
    {
        // Main initialization method called during application startup
        // Handles the overall seeding process by calling individual seeding methods
        // Parameters:
        //   - context: Database context for data operations
        //   - userManager: Identity user manager for user creation
        //   - roleManager: Identity role manager for role creation
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure the database exists and is created
            // This is useful for first run or when using an in-memory database
            context.Database.EnsureCreated();

            // Seed default application roles (Admin, Customer)
            await SeedRolesAsync(roleManager);

            // Seed default administrator account
            await SeedAdminUserAsync(userManager);

            // Seed default product categories
            await SeedCategoriesAsync(context);
        }

        // Creates default application roles if they don't already exist
        // Parameters:
        //   - roleManager: Identity role manager for role operations
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Define the role names for the application
            string[] roleNames = { "Admin", "Customer" };
            
            // Loop through each role name and create it if it doesn't exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the role in the ASP.NET Identity system
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        // Creates a default administrator account if it doesn't already exist
        // Parameters:
        //   - userManager: Identity user manager for user operations
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            // Check if the admin user already exists by email
            var adminUser = await userManager.FindByEmailAsync("admin@electronicsstore.com");
            
            // Only create admin if it doesn't exist
            if (adminUser == null)
            {
                // Create a new admin user with default values
                var user = new ApplicationUser
                {
                    UserName = "admin@electronicsstore.com",
                    Email = "admin@electronicsstore.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,    // Skip email confirmation
                    IsAdmin = true,           // Set the admin flag
                    Address = "123 Main St",
                    City = "Techville",
                    State = "CA",
                    PostalCode = "90210"
                };

                // Create the user with the specified password
                // In production, this password should be more secure and stored in configuration
                var result = await userManager.CreateAsync(user, "Admin123!");
                
                if (result.Succeeded)
                {
                    // Assign the Admin role to the newly created user
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        // Creates default product categories if none exist
        // Parameters:
        //   - context: Database context for category operations
        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            // Only seed categories if the Categories table is empty
            if (!context.Categories.Any())
            {
                // Define a list of initial categories with descriptions
                var categories = new List<Category>
                {
                    new Category { Name = "Smartphones", Description = "Mobile phones and accessories" },
                    new Category { Name = "Laptops", Description = "Notebook computers and accessories" },
                    new Category { Name = "Tablets", Description = "Tablet devices and accessories" },
                    new Category { Name = "Headphones", Description = "Wired and wireless headphones" },
                    new Category { Name = "Cameras", Description = "Digital cameras and accessories" },
                    new Category { Name = "Smart Home", Description = "Smart home devices and accessories" }
                };

                // Add all categories to the database at once
                context.Categories.AddRange(categories);
                
                // Save changes to the database
                await context.SaveChangesAsync();
            }
        }
    }
} 