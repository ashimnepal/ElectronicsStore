using ElectronicsStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Data
{
    /*
     * ApplicationDbContext is the central database context for the entire application
     * It extends IdentityDbContext to include ASP.NET Identity tables for authentication
     * This class defines all entity sets (tables) and configures their relationships
     * for proper database schema generation and navigation property behavior
     */
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor that accepts DbContextOptions configured in Program.cs/Startup.cs
        // These options include the database provider and connection string
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties represent database tables
        // Each property maps to a table in the database with the corresponding entity type
        
        // Products table storing all product information
        public DbSet<Product> Products { get; set; }
        
        // Categories table for product categorization
        public DbSet<Category> Categories { get; set; }
        
        // CartItems table for shopping cart functionality
        public DbSet<CartItem> CartItems { get; set; }
        
        // Orders table for storing customer orders
        public DbSet<Order> Orders { get; set; }
        
        // OrderItems table for storing items within each order
        public DbSet<OrderItem> OrderItems { get; set; }

        // Configure the database model relationships and constraints
        // This method is called when the model is being built
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call base implementation to set up Identity tables
            // This includes AspNetUsers, AspNetRoles, etc.
            base.OnModelCreating(modelBuilder);

            // Configure relationships between entities using Fluent API
            
            // Product belongs to one Category (many-to-one)
            // Each Category can have many Products (one-to-many)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)         // A product has one category
                .WithMany(c => c.Products)       // A category has many products
                .HasForeignKey(p => p.CategoryId); // The foreign key is CategoryId in the Product table

            // OrderItem belongs to one Order (many-to-one)
            // Each Order can have many OrderItems (one-to-many)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)         // An order item belongs to one order
                .WithMany(o => o.OrderItems)    // An order has many order items
                .HasForeignKey(oi => oi.OrderId); // The foreign key is OrderId in the OrderItem table

            // OrderItem refers to one Product (many-to-one)
            // The WithMany() without a parameter means we don't have a navigation property
            // from Product back to OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)       // An order item refers to one product
                .WithMany()                     // A product can be in many order items (no navigation property defined)
                .HasForeignKey(oi => oi.ProductId); // The foreign key is ProductId in the OrderItem table

            // Order belongs to one User (many-to-one)
            // The WithMany() without a parameter means we don't have a navigation property
            // from User back to Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)            // An order belongs to one user
                .WithMany()                     // A user can have many orders (no navigation property defined)
                .HasForeignKey(o => o.UserId);  // The foreign key is UserId in the Order table
        }
    }
} 