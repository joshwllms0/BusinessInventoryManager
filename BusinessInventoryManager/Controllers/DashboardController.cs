using BusinessInventoryManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products
                .CountAsync(p => p.IsActive);

            ViewBag.LowStockProducts = await _context.Products
                .CountAsync(p =>
                    p.IsActive &&
                    p.QuantityInStock < 5);

            ViewBag.OutOfStockProducts = await _context.Products
                .CountAsync(p =>
                    p.IsActive &&
                    p.QuantityInStock == 0);

            var inventoryValues = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.QuantityInStock,
                    p.UnitPrice
                })
                .ToListAsync();

            ViewBag.TotalInventoryValue = inventoryValues
                .Sum(p =>
                    p.QuantityInStock * p.UnitPrice);

            ViewBag.ActiveEmployees = User.IsInRole("Manager")
                ? await _context.Users.CountAsync(u => u.IsActive)
                : 0;

            return View();
        }
    }
}