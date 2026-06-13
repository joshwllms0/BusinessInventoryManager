using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessInventoryManager.Models
{
    public class Employee : Controller
    {
        public int EmpId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public Category Department { get; set; }

        public int? ManagerId { get; set; } // Null if they have no manager

        public Employee? Manager { get; set; }

        [Required]
        [Range(0.01, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>(); // List of who the manager is in charge of
    }
}
