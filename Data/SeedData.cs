using ElectronicsStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Seed roles
            await SeedRolesAsync(roleManager);

            // Seed admin user
            await SeedAdminUserAsync(userManager);

            // Seed categories
            await SeedCategoriesAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminUser = await userManager.FindByEmailAsync("admin@electronicsstore.com");
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@electronicsstore.com",
                    Email = "admin@electronicsstore.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsAdmin = true,
                    Address = "123 Main St",
                    City = "Techville",
                    State = "CA",
                    PostalCode = "90210"
                };

                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Smartphones", Description = "Mobile phones and accessories" },
                    new Category { Name = "Laptops", Description = "Notebook computers and accessories" },
                    new Category { Name = "Tablets", Description = "Tablet devices and accessories" },
                    new Category { Name = "Headphones", Description = "Wired and wireless headphones" },
                    new Category { Name = "Cameras", Description = "Digital cameras and accessories" },
                    new Category { Name = "Smart Home", Description = "Smart home devices and accessories" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }
    }
} 