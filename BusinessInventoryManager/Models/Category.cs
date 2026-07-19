using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}