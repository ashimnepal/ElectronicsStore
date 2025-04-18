using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }
        
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        // Navigation property for order items
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
    
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
} 