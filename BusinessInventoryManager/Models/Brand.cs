using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.Models
{
    public class Brand
    {
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}