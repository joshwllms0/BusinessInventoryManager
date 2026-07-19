using BusinessInventoryManager.Models;
using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.ViewModels.InventoryTransactions
{
    public class CreateTransactionViewModel
    {
        public int ProductId { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Current Stock")]
        public int CurrentQuantity { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public InventoryTransactionType TransactionType { get; set; }

        [Range(1, 999999)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}