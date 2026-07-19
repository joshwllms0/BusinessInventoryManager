using BusinessInventoryManager.Data;
using BusinessInventoryManager.Models;
using BusinessInventoryManager.ViewModels.InventoryTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Controllers
{
    [Authorize(Roles = "Manager,Employee")]
    public class InventoryTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InventoryTransactionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> History(
            int? productId,
            string? employeeId)
        {
            IQueryable<InventoryTransaction> query =
                _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(transaction => transaction.Product)
                    .Include(transaction => transaction.Employee);

            if (productId.HasValue)
            {
                query = query.Where(transaction =>
                    transaction.ProductId == productId.Value);
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                query = query.Where(transaction =>
                    transaction.EmployeeId == employeeId);
            }

            List<InventoryTransaction> transactions =
                await query
                    .OrderByDescending(transaction =>
                        transaction.TransactionDate)
                    .ToListAsync();

            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            Product? product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product =>
                    product.ProductId == productId &&
                    product.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            var model = new CreateTransactionViewModel
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                CurrentQuantity = product.QuantityInStock,
                TransactionType =
                    InventoryTransactionType.StockReceived
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateTransactionViewModel model)
        {
            Product? product = await _context.Products
                .FirstOrDefaultAsync(product =>
                    product.ProductId == model.ProductId &&
                    product.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            model.SKU = product.SKU;
            model.ProductName = product.ProductName;
            model.CurrentQuantity = product.QuantityInStock;

            int signedChange = GetSignedQuantityChange(
                model.TransactionType,
                model.Quantity);

            int newQuantity =
                product.QuantityInStock + signedChange;

            if (newQuantity < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    $"This transaction would reduce stock below zero. " +
                    $"Only {product.QuantityInStock} units are available.");
            }

            ApplicationUser? employee =
                await _userManager.GetUserAsync(User);

            if (employee == null || !employee.IsActive)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int previousQuantity = product.QuantityInStock;

            await using var databaseTransaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                product.QuantityInStock = newQuantity;
                product.UpdatedAt = DateTime.UtcNow;

                var inventoryTransaction =
                    new InventoryTransaction
                    {
                        ProductId = product.ProductId,
                        EmployeeId = employee.Id,
                        TransactionType =
                            model.TransactionType,
                        QuantityChange = signedChange,
                        PreviousQuantity = previousQuantity,
                        NewQuantity = newQuantity,
                        TransactionDate = DateTime.UtcNow,
                        Notes = model.Notes?.Trim()
                    };

                _context.InventoryTransactions.Add(
                    inventoryTransaction);

                await _context.SaveChangesAsync();
                await databaseTransaction.CommitAsync();
            }
            catch
            {
                await databaseTransaction.RollbackAsync();
                throw;
            }

            TempData["SuccessMessage"] =
                $"{product.ProductName} stock changed from " +
                $"{previousQuantity} to {newQuantity}.";

            return RedirectToAction(
                "Details",
                "Products",
                new { id = product.ProductId });
        }

        private static int GetSignedQuantityChange(
            InventoryTransactionType transactionType,
            int quantity)
        {
            return transactionType switch
            {
                InventoryTransactionType.StockReceived =>
                    quantity,

                InventoryTransactionType.AdjustmentIncrease =>
                    quantity,

                InventoryTransactionType.Returned =>
                    quantity,

                InventoryTransactionType.Sale =>
                    -quantity,

                InventoryTransactionType.AdjustmentDecrease =>
                    -quantity,

                InventoryTransactionType.Damaged =>
                    -quantity,

                InventoryTransactionType.Expired =>
                    -quantity,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(transactionType),
                    transactionType,
                    "Unsupported transaction type.")
            };
        }
    }
}