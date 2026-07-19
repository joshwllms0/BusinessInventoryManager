using BusinessInventoryManager.Models;

namespace BusinessInventoryManager.ViewModels.Employees
{
    public class EmployeeActivityReportViewModel
    {
        public string EmployeeId { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = "Unassigned";

        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int TotalTransactions { get; set; }

        public int TotalUnitsAdded { get; set; }

        public int TotalUnitsRemoved { get; set; }

        public int ProductsAffected { get; set; }

        public DateTime? FirstActivity { get; set; }

        public DateTime? LastActivity { get; set; }

        public IReadOnlyList<InventoryTransaction> Transactions
            { get; set; } = Array.Empty<InventoryTransaction>();
    }
}