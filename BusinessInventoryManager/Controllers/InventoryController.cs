using Microsoft.AspNetCore.Mvc;
using BusinessInventoryManager.Models;

namespace BusinessInventoryManager.Controllers
{
    public class InventoryController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var result = await //DatabaseName.Inventory.ToListAsync();
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                //Database.Inventory.Add(inventory)
                await //Database.SavechangesAsync()
                return RedirectToAction("Index");
            }
            return View(inventory);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = await //Database.Inventory.FindAsync(id.Value)

            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }

        public async Task<IActionResult> Edit (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = await //Database.Inventory.FindAsync(id.Value)

            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                //Database.Inventory.Update(inventory)
                await //Database.SaveChangesAsync()
                return RedirectToAction("Index");
            }
            return View(inventory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = await //Database.Inventory.FindAsync(id.Value);

            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await //Database.Inventory.FindAsync(id.Value);
            if (result == null)
            {
                return NotFound();
            }
            //Database.Remove(result);
            await //Database.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
