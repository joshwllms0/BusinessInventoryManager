using Microsoft.AspNetCore.Mvc;
using BusinessInventoryManager.Models;

namespace BusinessInventoryManager.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
