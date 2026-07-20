using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Department)
                .OrderBy(c => c.Department.DepartmentName)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDepartmentsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            bool duplicate = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryName == category.CategoryName.Trim() &&
                    c.DepartmentId == category.DepartmentId);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(category.CategoryName),
                    "This category name already exists in the selected department.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(category.DepartmentId);
                return View(category);
            }

            category.CategoryName = category.CategoryName.Trim();
            category.Description = category.Description?.Trim();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{category.CategoryName} was created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return NotFound();

            Category? category = await _context.Categories
                .Include(c => c.Department)
                .Include(c => c.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.CategoryId == id.Value);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            Category? category = await _context.Categories
                .FindAsync(id.Value);

            if (category == null) return NotFound();

            await PopulateDepartmentsAsync(category.DepartmentId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId) return BadRequest();

            bool duplicate = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryName == category.CategoryName.Trim() &&
                    c.DepartmentId == category.DepartmentId &&
                    c.CategoryId != id);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(category.CategoryName),
                    "This category name already exists in the selected department.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(category.DepartmentId);
                return View(category);
            }

            Category? existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.CategoryName = category.CategoryName.Trim();
            existing.Description = category.Description?.Trim();
            existing.DepartmentId = category.DepartmentId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{existing.CategoryName} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            Category? category = await _context.Categories
                .Include(c => c.Department)
                .Include(c => c.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.CategoryId == id.Value);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Category? category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            if (category.Products.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete {category.CategoryName} because it has products assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{category.CategoryName} was deleted.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDepartmentsAsync(int selectedId = 0)
        {
            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName,
                    Selected = d.DepartmentId == selectedId
                })
                .ToListAsync();
        }
    }
}