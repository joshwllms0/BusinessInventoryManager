using System.ComponentModel.DataAnnotations;

namespace BusinessInventoryManager.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        public ApplicationUser Employee { get; set; } = null!;

        public InventoryTransactionType TransactionType { get; set; }

        public int QuantityChange { get; set; }

        public int PreviousQuantity { get; set; }

        public int NewQuantity { get; set; }

        public DateTime TransactionDate { get; set; }
            = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}