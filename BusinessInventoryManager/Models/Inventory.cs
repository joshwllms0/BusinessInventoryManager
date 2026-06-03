using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string ItemName { get; set; } = string.Empty;
        [StringLength(1000)]
        public string? ItemDescription { get; set; } = null;
        public Category Category { get; set; }
        [Required]
        [Range(0, 99999)]
        public int Stock { get; set; }
        [Required]
        [Range(0.01, 9999)]
        public decimal Price { get; set; }
    }

    public enum Category
    {
        Electronics,
        Clothing,
        Books,
        Food
        // Etc.
    }
}
