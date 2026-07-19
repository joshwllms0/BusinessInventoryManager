using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.ViewModels.Employees
{
    public class CreateEmployeeViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "The password must contain at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Employee";

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Manager")]
        public string? ManagerId { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Managers { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Roles { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}