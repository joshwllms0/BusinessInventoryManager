using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        public ICollection<ApplicationUser> Employees { get; set; }
            = new List<ApplicationUser>();
    }
}