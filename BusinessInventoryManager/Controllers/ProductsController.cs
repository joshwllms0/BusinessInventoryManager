using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using BusinessInventoryManager.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(product => product.Brand)
                .Include(product => product.Category)
                    .ThenInclude(category => category.Department)
                .Where(product => product.IsActive)
                .OrderBy(product => product.ProductName)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            Product? product = await _context.Products
                .AsNoTracking()
                .Include(product => product.Brand)
                .Include(product => product.Category)
                    .ThenInclude(category => category.Department)
                .FirstOrDefaultAsync(product =>
                    product.ProductId == id.Value);

            if (product == null)
            {
                return NotFound();
            }

            List<InventoryTransaction> recentTransactions =
                await _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(transaction => transaction.Employee)
                    .Where(transaction =>
                        transaction.ProductId == product.ProductId)
                    .OrderByDescending(transaction =>
                        transaction.TransactionDate)
                    .Take(10)
                    .ToListAsync();

            product.Transactions = recentTransactions;

            return View(product);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormViewModel();

            await PopulateSelectionsAsync(model);

            return View(model);
        }
        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            Product? product = await _context.Products
                .Include(item => item.Category)
                .FirstOrDefaultAsync(item =>
                    item.ProductId == id.Value);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductFormViewModel
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                Description = product.Description,
                DepartmentId = product.Category.DepartmentId,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Flavor = product.Flavor,
                Size = product.Size,
                UnitOfMeasure = product.UnitOfMeasure,
                UnitPrice = product.UnitPrice,
                QuantityInStock = product.QuantityInStock,
                ReorderLevel = product.ReorderLevel
            };

            await PopulateSelectionsAsync(model);

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductFormViewModel model)
        {
            if (id != model.ProductId)
            {
                return BadRequest();
            }

            Product? product = await _context.Products
                .FirstOrDefaultAsync(item =>
                    item.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            bool duplicateSku = await _context.Products
                .AnyAsync(item =>
                    item.SKU == model.SKU.Trim() &&
                    item.ProductId != id);

            if (duplicateSku)
            {
                ModelState.AddModelError(
                    nameof(model.SKU),
                    "Another product already uses this SKU.");
            }

            if (model.CategoryId.HasValue &&
                model.DepartmentId.HasValue)
            {
                bool validCategory = await _context.Categories
                    .AnyAsync(category =>
                        category.CategoryId == model.CategoryId.Value &&
                        category.DepartmentId == model.DepartmentId.Value);

                if (!validCategory)
                {
                    ModelState.AddModelError(
                        nameof(model.CategoryId),
                        "The selected category does not belong to the selected department.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(model);
                return View(model);
            }

            product.SKU = model.SKU.Trim();
            product.ProductName = model.ProductName.Trim();
            product.Description = model.Description?.Trim();
            product.BrandId = model.BrandId!.Value;
            product.CategoryId = model.CategoryId!.Value;
            product.Flavor = model.Flavor?.Trim();
            product.Size = model.Size?.Trim();
            product.UnitOfMeasure = model.UnitOfMeasure?.Trim();
            product.UnitPrice = model.UnitPrice;
            product.QuantityInStock = model.QuantityInStock;
            product.ReorderLevel = model.ReorderLevel;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{product.ProductName} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            Product? product = await _context.Products
                .Include(item => item.Brand)
                .Include(item => item.Category)
                    .ThenInclude(category => category.Department)
                .FirstOrDefaultAsync(item =>
                    item.ProductId == id.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Product? product = await _context.Products
                .FirstOrDefaultAsync(item =>
                    item.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{product.ProductName} was removed from the active product list.";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProductFormViewModel model)
        {
            if (await _context.Products.AnyAsync(product =>
                product.SKU == model.SKU.Trim()))
            {
                ModelState.AddModelError(
                    nameof(model.SKU),
                    "A product with this SKU already exists.");
            }

            if (model.CategoryId.HasValue &&
                model.DepartmentId.HasValue)
            {
                bool validCategory = await _context.Categories
                    .AnyAsync(category =>
                        category.CategoryId == model.CategoryId &&
                        category.DepartmentId == model.DepartmentId);

                if (!validCategory)
                {
                    ModelState.AddModelError(
                        nameof(model.CategoryId),
                        "The selected category does not belong to the selected department.");
                }
            }


            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(model);
                return View(model);
            }

            var product = new Product
            {
                SKU = model.SKU.Trim(),
                ProductName = model.ProductName.Trim(),
                Description = model.Description?.Trim(),
                BrandId = model.BrandId!.Value,
                CategoryId = model.CategoryId!.Value,
                Flavor = model.Flavor?.Trim(),
                Size = model.Size?.Trim(),
                UnitOfMeasure = model.UnitOfMeasure?.Trim(),
                UnitPrice = model.UnitPrice,
                QuantityInStock = model.QuantityInStock,
                ReorderLevel = model.ReorderLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{product.ProductName} was created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CategoriesByDepartment(
            int departmentId)
        {
            var categories = await _context.Categories
                .Where(category =>
                    category.DepartmentId == departmentId)
                .OrderBy(category => category.CategoryName)
                .Select(category => new
                {
                    value = category.CategoryId,
                    text = category.CategoryName
                })
                .ToListAsync();

            return Json(categories);
        }

        [HttpGet]
    

        private async Task PopulateSelectionsAsync(
            ProductFormViewModel model)
        {
            model.Departments = await _context.Departments
                .OrderBy(department => department.DepartmentName)
                .Select(department => new SelectListItem
                {
                    Value = department.DepartmentId.ToString(),
                    Text = department.DepartmentName
                })
                .ToListAsync();

            model.Brands = await _context.Brands
                .OrderBy(brand => brand.BrandName)
                .Select(brand => new SelectListItem
                {
                    Value = brand.BrandId.ToString(),
                    Text = brand.BrandName
                })
                .ToListAsync();

            model.Categories = model.DepartmentId.HasValue
                ? await _context.Categories
                    .Where(category =>
                        category.DepartmentId ==
                        model.DepartmentId.Value)
                    .OrderBy(category => category.CategoryName)
                    .Select(category => new SelectListItem
                    {
                        Value = category.CategoryId.ToString(),
                        Text = category.CategoryName
                    })
                    .ToListAsync()
                : Enumerable.Empty<SelectListItem>();

            model.Units = new[]
            {
                new SelectListItem("Each", "each"),
                new SelectListItem("Ounces", "oz"),
                new SelectListItem("Pounds", "lb"),
                new SelectListItem("Gallons", "gal"),
                new SelectListItem("Liters", "L"),
                new SelectListItem("Milliliters", "mL"),
                new SelectListItem("Count", "count"),
                new SelectListItem("Package", "package")
            };
        }
    }
}