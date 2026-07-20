using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands
                .OrderBy(b => b.BrandName)
                .ToListAsync();

            return View(brands);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            bool duplicate = await _context.Brands
                .AnyAsync(b => b.BrandName == brand.BrandName.Trim());

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(brand.BrandName),
                    "A brand with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            brand.BrandName = brand.BrandName.Trim();
            brand.Description = brand.Description?.Trim();

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{brand.BrandName} was created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return NotFound();

            Brand? brand = await _context.Brands
                .Include(b => b.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(b => b.BrandId == id.Value);

            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            Brand? brand = await _context.Brands
                .FindAsync(id.Value);

            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.BrandId) return BadRequest();

            bool duplicate = await _context.Brands
                .AnyAsync(b =>
                    b.BrandName == brand.BrandName.Trim() &&
                    b.BrandId != id);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(brand.BrandName),
                    "A brand with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            Brand? existing = await _context.Brands.FindAsync(id);
            if (existing == null) return NotFound();

            existing.BrandName = brand.BrandName.Trim();
            existing.Description = brand.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{existing.BrandName} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            Brand? brand = await _context.Brands
                .Include(b => b.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(b => b.BrandId == id.Value);

            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Brand? brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.BrandId == id);

            if (brand == null) return NotFound();

            if (brand.Products.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete {brand.BrandName} because it has products assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{brand.BrandName} was deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}