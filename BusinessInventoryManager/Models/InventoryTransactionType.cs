namespace BusinessInventoryManager.Models
{
    public enum InventoryTransactionType
    {
        StockReceived = 1,
        Sale = 2,
        AdjustmentIncrease = 3,
        AdjustmentDecrease = 4,
        Damaged = 5,
        Expired = 6,
        Returned = 7
    }
}