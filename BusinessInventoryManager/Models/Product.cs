using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int BrandId { get; set; }

        public Brand Brand { get; set; } = null!;

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;


        [StringLength(100)]
        public string? Flavor { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [StringLength(30)]
        public string? UnitOfMeasure { get; set; }

        [Range(0.01, 99999)]
        public decimal UnitPrice { get; set; }

        [Range(0, 999999)]
        public int QuantityInStock { get; set; }

        [Range(0, 999999)]
        public int ReorderLevel { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<InventoryTransaction> Transactions { get; set; }
            = new List<InventoryTransaction>();

        public bool IsLowStock =>
            QuantityInStock < 5;

        public decimal InventoryValue =>
            QuantityInStock * UnitPrice;
    }
}