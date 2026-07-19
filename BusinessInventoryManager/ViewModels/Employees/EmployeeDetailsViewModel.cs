using BusinessInventoryManager.Models;

namespace BusinessInventoryManager.ViewModels.Employees
{
    public class EmployeeDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = "Unassigned";

        public string ManagerName { get; set; } = "No assigned manager";

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TransactionCount { get; set; }

        public DateTime? LastActivity { get; set; }

        public IReadOnlyList<InventoryTransaction> RecentTransactions
            { get; set; } = Array.Empty<InventoryTransaction>();
    }
}