using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            bool duplicate = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentName == department.DepartmentName.Trim());

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(department.DepartmentName),
                    "A department with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            department.DepartmentName = department.DepartmentName.Trim();
            department.Description = department.Description?.Trim();

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{department.DepartmentName} was created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return NotFound();

            Department? department = await _context.Departments
                .Include(d => d.Categories)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.DepartmentId == id.Value);

            if (department == null) return NotFound();

            return View(department);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            Department? department = await _context.Departments
                .FindAsync(id.Value);

            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId) return BadRequest();

            bool duplicate = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentName == department.DepartmentName.Trim() &&
                    d.DepartmentId != id);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(department.DepartmentName),
                    "A department with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            Department? existing = await _context.Departments
                .FindAsync(id);

            if (existing == null) return NotFound();

            existing.DepartmentName = department.DepartmentName.Trim();
            existing.Description = department.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{existing.DepartmentName} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            Department? department = await _context.Departments
                .Include(d => d.Categories)
                .FirstOrDefaultAsync(d => d.DepartmentId == id.Value);

            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Department? department = await _context.Departments
                .Include(d => d.Categories)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null) return NotFound();

            if (department.Categories.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete {department.DepartmentName} because it has " +
                    $"{department.Categories.Count} category/categories assigned to it. " +
                    "Remove those categories first.";

                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{department.DepartmentName} was deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}