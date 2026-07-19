using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using BusinessInventoryManager.ViewModels.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BusinessInventoryManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class EmployeeManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public async Task<IActionResult> Index()
        {
            List<ApplicationUser> users = await _context.Users
                .Include(user => user.Department)
                .Include(user => user.Manager)
                .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                .ToListAsync();

            var results = new List<EmployeeDetailsViewModel>();

            foreach (ApplicationUser user in users)
            {
                IList<string> roles =
                    await _userManager.GetRolesAsync(user);

               results.Add(new EmployeeDetailsViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "Unassigned",
                    DepartmentName =
                        user.Department?.DepartmentName ?? "Unassigned",
                    ManagerName =
                        user.Manager?.FullName ?? "Unassigned",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    TransactionCount =
                        await _context.InventoryTransactions.CountAsync(
                            transaction =>
                                transaction.EmployeeId == user.Id),
                    LastActivity =
                        await _context.InventoryTransactions
                            .Where(transaction =>
                                transaction.EmployeeId == user.Id)
                            .MaxAsync(transaction =>
                                (DateTime?)transaction.TransactionDate)
                });
            }

            return View(results);
        }
        [HttpGet]
        public async Task<IActionResult> Report(
            string? id,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ApplicationUser? employee = await _context.Users
                .AsNoTracking()
                .Include(user => user.Department)
                .FirstOrDefaultAsync(user => user.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            if (startDate.HasValue &&
                endDate.HasValue &&
                startDate.Value.Date > endDate.Value.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The start date cannot be later than the end date.");
            }

            IQueryable<InventoryTransaction> query =
                _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(transaction => transaction.Product)
                    .Where(transaction =>
                        transaction.EmployeeId == employee.Id);

            if (startDate.HasValue)
            {
                DateTime rangeStart = startDate.Value.Date;

                query = query.Where(transaction =>
                    transaction.TransactionDate >= rangeStart);
            }

            if (endDate.HasValue)
            {
                DateTime rangeEnd =
                    endDate.Value.Date.AddDays(1);

                query = query.Where(transaction =>
                    transaction.TransactionDate < rangeEnd);
            }

            List<InventoryTransaction> transactions =
                await query
                    .OrderByDescending(transaction =>
                        transaction.TransactionDate)
                    .ToListAsync();

            IList<string> roles =
                await _userManager.GetRolesAsync(employee);

            var model = new EmployeeActivityReportViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                Email = employee.Email ??
                        employee.UserName ??
                        "No email",
                Role = roles.FirstOrDefault() ?? "No role",
                DepartmentName =
                    employee.Department?.DepartmentName ??
                    "Unassigned",
                IsActive = employee.IsActive,
                StartDate = startDate,
                EndDate = endDate,
                TotalTransactions = transactions.Count,
                TotalUnitsAdded = transactions
                    .Where(transaction =>
                        transaction.QuantityChange > 0)
                    .Sum(transaction =>
                        transaction.QuantityChange),
                TotalUnitsRemoved = transactions
                    .Where(transaction =>
                        transaction.QuantityChange < 0)
                    .Sum(transaction =>
                        Math.Abs(transaction.QuantityChange)),
                ProductsAffected = transactions
                    .Select(transaction =>
                        transaction.ProductId)
                    .Distinct()
                    .Count(),
                FirstActivity = transactions
                    .OrderBy(transaction =>
                        transaction.TransactionDate)
                    .Select(transaction =>
                        (DateTime?)transaction.TransactionDate)
                    .FirstOrDefault(),
                LastActivity = transactions
                    .OrderByDescending(transaction =>
                        transaction.TransactionDate)
                    .Select(transaction =>
                        (DateTime?)transaction.TransactionDate)
                    .FirstOrDefault(),
                Transactions = transactions
            };

            return View(model);
        }
        
       [HttpGet]
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ApplicationUser? employee = await _context.Users
                .AsNoTracking()
                .Include(user => user.Department)
                .Include(user => user.Manager)
                .FirstOrDefaultAsync(user => user.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            IList<string> roles =
                await _userManager.GetRolesAsync(employee);

            List<InventoryTransaction> recentTransactions =
                await _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(transaction => transaction.Product)
                    .Where(transaction =>
                        transaction.EmployeeId == employee.Id)
                    .OrderByDescending(transaction =>
                        transaction.TransactionDate)
                    .Take(10)
                    .ToListAsync();

            int transactionCount =
                await _context.InventoryTransactions
                    .CountAsync(transaction =>
                        transaction.EmployeeId == employee.Id);

            DateTime? lastActivity =
                await _context.InventoryTransactions
                    .Where(transaction =>
                        transaction.EmployeeId == employee.Id)
                    .OrderByDescending(transaction =>
                        transaction.TransactionDate)
                    .Select(transaction =>
                        (DateTime?)transaction.TransactionDate)
                    .FirstOrDefaultAsync();

            var model = new EmployeeDetailsViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email ?? employee.UserName ?? "No email",
                Role = roles.FirstOrDefault() ?? "No role",
                DepartmentName =
                    employee.Department?.DepartmentName ?? "Unassigned",
                ManagerName =
                    employee.Manager?.FullName ?? "No assigned manager",
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt,
                TransactionCount = transactionCount,
                LastActivity = lastActivity,
                RecentTransactions = recentTransactions
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateEmployeeViewModel();

            await PopulateSelectionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateEmployeeViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "The selected role does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(model);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                DepartmentId = model.DepartmentId,
                ManagerId = model.ManagerId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            IdentityResult result =
                await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                AddIdentityErrors(result);

                await PopulateSelectionsAsync(model);
                return View(model);
            }

            IdentityResult roleResult =
                await _userManager.AddToRoleAsync(user, model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                AddIdentityErrors(roleResult);

                await PopulateSelectionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] =
                $"{user.FullName} was created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ApplicationUser? user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            IList<string> roles =
                await _userManager.GetRolesAsync(user);

            var model = new EditEmployeeViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Employee",
                DepartmentId = user.DepartmentId,
                ManagerId = user.ManagerId,
                IsActive = user.IsActive
            };

            await PopulateSelectionsAsync(model, user.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            EditEmployeeViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (model.ManagerId == model.Id)
            {
                ModelState.AddModelError(
                    nameof(model.ManagerId),
                    "An employee cannot manage their own account.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(model, model.Id);
                return View(model);
            }

            ApplicationUser? user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.Email = model.Email.Trim();
            user.UserName = model.Email.Trim();
            user.DepartmentId = model.DepartmentId;
            user.ManagerId = model.ManagerId;
            user.IsActive = model.IsActive;

            IdentityResult updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                AddIdentityErrors(updateResult);

                await PopulateSelectionsAsync(model, model.Id);
                return View(model);
            }

            IList<string> currentRoles =
                await _userManager.GetRolesAsync(user);

            if (!currentRoles.Contains(model.Role))
            {
                if (currentRoles.Count > 0)
                {
                    IdentityResult removeResult =
                        await _userManager.RemoveFromRolesAsync(
                            user,
                            currentRoles);

                    if (!removeResult.Succeeded)
                    {
                        AddIdentityErrors(removeResult);

                        await PopulateSelectionsAsync(model, model.Id);
                        return View(model);
                    }
                }

                IdentityResult addResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        model.Role);

                if (!addResult.Succeeded)
                {
                    AddIdentityErrors(addResult);

                    await PopulateSelectionsAsync(model, model.Id);
                    return View(model);
                }
            }

            TempData["SuccessMessage"] =
                $"{user.FullName} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            ApplicationUser? user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (_userManager.GetUserId(User) == user.Id)
            {
                TempData["ErrorMessage"] =
                    "You cannot delete your own account.";

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            ApplicationUser? user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (_userManager.GetUserId(User) == user.Id)
            {
                TempData["ErrorMessage"] =
                    "You cannot delete your own account.";

                return RedirectToAction(nameof(Index));
            }

            bool hasSubordinates =
                await _context.Users.AnyAsync(
                    employee => employee.ManagerId == user.Id);

            if (hasSubordinates)
            {
                TempData["ErrorMessage"] =
                    "Reassign this manager's employees before deletion.";

                return RedirectToAction(nameof(Index));
            }

            IList<string> roles =
                await _userManager.GetRolesAsync(user);

            if (roles.Contains("Manager"))
            {
                int managerCount = 0;

                List<ApplicationUser> activeUsers =
                    await _context.Users
                        .Where(item => item.IsActive)
                        .ToListAsync();

                foreach (ApplicationUser activeUser in activeUsers)
                {
                    if (await _userManager.IsInRoleAsync(
                        activeUser,
                        "Manager"))
                    {
                        managerCount++;
                    }
                }

                if (managerCount <= 1)
                {
                    TempData["ErrorMessage"] =
                        "The final active manager cannot be deleted.";

                    return RedirectToAction(nameof(Index));
                }
            }

            bool hasTransactions =
                await _context.InventoryTransactions.AnyAsync(
                    transaction =>
                        transaction.EmployeeId == user.Id);

            if (hasTransactions)
            {
                user.IsActive = false;

                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] =
                    "The account has transaction history and was deactivated instead of deleted.";

                return RedirectToAction(nameof(Index));
            }

            IdentityResult result =
                await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    string.Join(
                        " ",
                        result.Errors.Select(error =>
                            error.Description));

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "The employee account was deleted.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectionsAsync(
            CreateEmployeeViewModel model)
        {
            model.Departments =
                await GetDepartmentOptionsAsync();

            model.Managers =
                await GetManagerOptionsAsync();

            model.Roles = GetRoleOptions();
        }

        private async Task PopulateSelectionsAsync(
            EditEmployeeViewModel model,
            string excludedUserId)
        {
            model.Departments =
                await GetDepartmentOptionsAsync();

            model.Managers =
                await GetManagerOptionsAsync(excludedUserId);

            model.Roles = GetRoleOptions();
        }

        private async Task<IEnumerable<SelectListItem>>
            GetDepartmentOptionsAsync()
        {
            return await _context.Departments
                .OrderBy(department =>
                    department.DepartmentName)
                .Select(department =>
                    new SelectListItem
                    {
                        Value =
                            department.DepartmentId.ToString(),
                        Text = department.DepartmentName
                    })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>>
            GetManagerOptionsAsync(
                string? excludedUserId = null)
        {
            List<ApplicationUser> users =
                await _context.Users
                    .Where(user =>
                        user.IsActive &&
                        user.Id != excludedUserId)
                    .OrderBy(user => user.LastName)
                    .ThenBy(user => user.FirstName)
                    .ToListAsync();

            var options = new List<SelectListItem>();

            foreach (ApplicationUser user in users)
            {
                if (await _userManager.IsInRoleAsync(
                    user,
                    "Manager"))
                {
                    options.Add(new SelectListItem
                    {
                        Value = user.Id,
                        Text = user.FullName
                    });
                }
            }

            return options;
        }

        private static IEnumerable<SelectListItem>
            GetRoleOptions()
        {
            return new[]
            {
                new SelectListItem
                {
                    Value = "Employee",
                    Text = "Employee"
                },
                new SelectListItem
                {
                    Value = "Manager",
                    Text = "Manager"
                }
            };
        }

        private void AddIdentityErrors(
            IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }
        }
    }
}