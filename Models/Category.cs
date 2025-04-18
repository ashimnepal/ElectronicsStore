using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Navigation property for products in this category
        public ICollection<Product>? Products { get; set; }
    }
} 