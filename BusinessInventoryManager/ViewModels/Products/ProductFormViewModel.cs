using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.ViewModels.Products
{
    public class ProductFormViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Brand")]
        public int? BrandId { get; set; }

        [StringLength(100)]
        public string? Flavor { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [StringLength(30)]
        [Display(Name = "Unit of Measure")]
        public string? UnitOfMeasure { get; set; }

        [Range(0.01, 99999)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Range(0, 999999)]
        [Display(Name = "Initial Stock")]
        public int QuantityInStock { get; set; }

        [Range(0, 999999)]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; } = 5;

        public IEnumerable<SelectListItem> Departments { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Categories { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Brands { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Units { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}