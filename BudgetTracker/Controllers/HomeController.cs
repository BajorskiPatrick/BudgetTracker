using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Models;
using BudgetTracker.Data;
using BudgetTracker.ViewModels;

namespace BudgetTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(int? year = null, int? month = null)
    {
        // Sprawdź czy użytkownik jest zalogowany
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
        {
            // Jeśli nie zalogowany, pokaż podstawową stronę
            return View(new DashboardViewModel());
        }

        // Ustaw domyślne wartości na aktualny miesiąc
        var selectedYear = year ?? DateTime.Now.Year;
        var selectedMonth = month ?? DateTime.Now.Month;

        var viewModel = await GetDashboardData(currentUserId, selectedYear, selectedMonth);
        
        return View(viewModel);
    }

    private async Task<DashboardViewModel> GetDashboardData(long userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // 1. Pobranie łącznych przychodów i wydatków
        var totalIncome = await _context.Income
            .Where(i => i.UserId == userId && i.TransactionDate >= startDate && i.TransactionDate <= endDate)
            .SumAsync(i => i.Amount);

        var totalExpenses = await _context.Expense
            .Where(e => e.UserId == userId && e.TransactionDate >= startDate && e.TransactionDate <= endDate)
            .SumAsync(e => e.Amount);

        // 2. Wydatki według kategorii
        var expensesByCategory = await _context.Expense
            .Include(e => e.Category)
            .Where(e => e.UserId == userId && e.TransactionDate >= startDate && e.TransactionDate <= endDate)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        // 3. Przychody według kategorii
        var incomesByCategory = await _context.Income
            .Include(i => i.Category)
            .Where(i => i.UserId == userId && i.TransactionDate >= startDate && i.TransactionDate <= endDate)
            .GroupBy(i => i.Category!.Name)
            .Select(g => new CategorySummary
            {
                CategoryName = g.Key,
                Amount = g.Sum(i => i.Amount)
            })
            .ToListAsync();

        // 4. Wydatki według metod płatności
        var expensesByPaymentMethod = await _context.Expense
            .Include(e => e.PaymentMethod)
            .Where(e => e.UserId == userId && e.TransactionDate >= startDate && e.TransactionDate <= endDate)
            .GroupBy(e => e.PaymentMethod!.Name)
            .Select(g => new PaymentMethodSummary
            {
                PaymentMethodName = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        // 5. Porównanie wydatków z limitami
        var limitsComparison = await GetLimitsComparison(userId, year, month);

        // 6. Dostępne lata i miesiące dla selektora
        var availableYears = await _context.Expense
            .Where(e => e.UserId == userId)
            .Select(e => e.TransactionDate.Year)
            .Union(_context.Income.Where(i => i.UserId == userId).Select(i => i.TransactionDate.Year))
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        if (!availableYears.Any())
        {
            availableYears.Add(DateTime.Now.Year);
        }

        return new DashboardViewModel
        {
            IsUserLoggedIn = true,
            SelectedYear = year,
            SelectedMonth = month,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            Balance = totalIncome - totalExpenses,
            ExpensesByCategory = expensesByCategory,
            IncomesByCategory = incomesByCategory,
            ExpensesByPaymentMethod = expensesByPaymentMethod,
            LimitsComparison = limitsComparison,
            AvailableYears = availableYears
        };
    }

    private async Task<List<LimitComparison>> GetLimitsComparison(long userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Pobierz wszystkie limity użytkownika
        var limits = await _context.Limit
            .Include(l => l.Category)
            .Where(l => l.UserId == userId)
            .ToListAsync();

        var result = new List<LimitComparison>();

        foreach (var limit in limits)
        {
            // Oblicz wydatki w danej kategorii w wybranym miesiącu
            var expensesInCategory = await _context.Expense
                .Where(e => e.UserId == userId && 
                           e.CategoryId == limit.CategoryId && 
                           e.TransactionDate >= startDate && 
                           e.TransactionDate <= endDate)
                .SumAsync(e => e.Amount);

            var percentage = limit.Amount > 0 ? (expensesInCategory / limit.Amount) * 100 : 0;
            
            // Określ status na podstawie procentu wykorzystania
            var status = LimitStatus.Safe;
            if (percentage >= 100)
                status = LimitStatus.Exceeded;
            else if (percentage >= 80)
                status = LimitStatus.Warning;

            result.Add(new LimitComparison
            {
                CategoryName = limit.Category!.Name,
                LimitAmount = limit.Amount,
                SpentAmount = expensesInCategory,
                RemainingAmount = limit.Amount - expensesInCategory,
                PercentageUsed = percentage,
                Status = status
            });
        }

        return result.OrderByDescending(l => l.PercentageUsed).ToList();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}