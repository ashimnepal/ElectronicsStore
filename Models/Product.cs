using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
} 