using BusinessInventoryManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products
                .CountAsync(product => product.IsActive);

            ViewBag.LowStockCount = await _context.Products
                .CountAsync(product =>
                    product.IsActive &&
                    product.QuantityInStock < 5);

            ViewBag.OutOfStockCount = await _context.Products
                .CountAsync(product =>
                    product.IsActive &&
                    product.QuantityInStock == 0);

            var values = await _context.Products
                .Where(product => product.IsActive)
                .Select(product => new
                {
                    product.QuantityInStock,
                    product.UnitPrice
                })
                .ToListAsync();

            ViewBag.TotalInventoryValue = values.Sum(product =>
                product.QuantityInStock * product.UnitPrice);

            return View();
        }

        public async Task<IActionResult> LowStock()
        {
            var products = await _context.Products
                .Include(product => product.Brand)
                .Include(product => product.Category)
                    .ThenInclude(category => category.Department)
                .Where(product =>
                    product.IsActive &&
                    product.QuantityInStock < 5)
                .OrderBy(product => product.QuantityInStock)
                .ThenBy(product => product.ProductName)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> OutOfStock()
        {
            var products = await _context.Products
                .Include(product => product.Brand)
                .Include(product => product.Category)
                    .ThenInclude(category => category.Department)
                .Where(product =>
                    product.IsActive &&
                    product.QuantityInStock == 0)
                .OrderBy(product => product.ProductName)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Category(int? categoryId)
        {
            ViewBag.Categories = await _context.Categories
                .Include(category => category.Department)
                .OrderBy(category => category.Department.DepartmentName)
                .ThenBy(category => category.CategoryName)
                .ToListAsync();

            var query = _context.Products
                .Include(product => product.Brand)
                .Include(product => product.Category)
                    .ThenInclude(category => category.Department)
                .Where(product => product.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(product =>
                    product.CategoryId == categoryId.Value);
            }

            ViewBag.SelectedCategoryId = categoryId;

            return View(await query
                .OrderBy(product => product.ProductName)
                .ToListAsync());
        }
    }
}