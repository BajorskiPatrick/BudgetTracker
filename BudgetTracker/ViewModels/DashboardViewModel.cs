namespace BudgetTracker.ViewModels
{
    public class DashboardViewModel
    {
        public bool IsUserLoggedIn { get; set; } = false;
        public int? SelectedYear { get; set; }
        public int? SelectedMonth { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public List<CategorySummary> ExpensesByCategory { get; set; } = new();
        public List<CategorySummary> IncomesByCategory { get; set; } = new();
        public List<PaymentMethodSummary> ExpensesByPaymentMethod { get; set; } = new();
        public List<LimitComparison> LimitsComparison { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
    }

    public class CategorySummary
    {
        public string CategoryName { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class PaymentMethodSummary
    {
        public string PaymentMethodName { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class LimitComparison
    {
        public string CategoryName { get; set; } = "";
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PercentageUsed { get; set; }
        public LimitStatus Status { get; set; }
    }

    public enum LimitStatus
    {
        Safe,       // < 80% limitu
        Warning,    // 80-99% limitu  
        Exceeded    // >= 100% limitu
    }
}