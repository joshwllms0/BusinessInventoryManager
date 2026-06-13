using BusinessInventoryManager.Data;
using Microsoft.AspNetCore.Mvc;
using BusinessInventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly InventoryContext _context;

        public EmployeeController(InventoryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.Include(e => e.Manager).ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees.Include(e => e.Manager)
                .Include(e => e.Subordinates).FirstOrDefaultAsync(e => e.EmpId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Managers = await _context.Employees.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await _context.Employees.ToListAsync();
                return View(employee);
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Managers = await _context.Employees.Where(e => e.EmpId != id).ToListAsync();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.EmpId) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await _context.Employees
                    .Where(e => e.EmpId != id)
                    .ToListAsync();
                return View(employee);
            }

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.EmpId == id);

            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
