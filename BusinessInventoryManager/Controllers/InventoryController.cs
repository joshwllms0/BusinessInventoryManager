using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryContext _context;

        public InventoryController(InventoryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, Category? category, string? sort)
        {
            var query = _context.Inventory.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => i.ItemName.Contains(search));

            if (category.HasValue)
                query = query.Where(i => i.Category == category.Value);

            query = sort switch
            {
                "stock_asc" => query.OrderBy(i => i.Stock),
                "price_desc" => query.OrderByDescending(i => (double)i.Price),
                _ => query.OrderBy(i => i.ItemName)
            };

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSort = sort;

            return View(await query.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                _context.Inventory.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var result = await _context.Inventory.FindAsync(id.Value);
            if (result == null) return NotFound();
            return View(result);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var result = await _context.Inventory.FindAsync(id.Value);
            if (result == null) return NotFound();
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                _context.Inventory.Update(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var result = await _context.Inventory.FindAsync(id.Value);
            if (result == null) return NotFound();
            return View(result);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmation(int? id)
        {
            if (id == null) return NotFound();
            var result = await _context.Inventory.FindAsync(id.Value);
            if (result == null) return NotFound();
            _context.Inventory.Remove(result);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}