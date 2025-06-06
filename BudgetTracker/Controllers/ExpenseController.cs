using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Utils;

namespace BudgetTracker.Controllers
{
    [TypeFilter(typeof(AuthorizationFilter))]
    [TypeFilter(typeof(UserOnlyFilter))]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expense
        public async Task<IActionResult> Index(long? categoryFilter, long? paymentMethodFilter, int? yearFilter, int? monthFilter)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Pobierz listy do filtrów (potrzebne dla widoku)
            var categories = await _context.Category
                .Where(c => c.UserId == currentUserId && c.Type == CategoryType.Expense)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            var paymentMethods = await _context.PaymentMethod
                .Where(p => p.UserId == currentUserId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Przygotuj zapytanie podstawowe
            IQueryable<Expense> expensesQuery = _context.Expense
                .Include(e => e.Category)
                .Include(e => e.PaymentMethod)
                .Where(e => e.UserId == currentUserId);

            // Zastosuj filtry jeśli zostały podane
            if (categoryFilter.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.CategoryId == categoryFilter.Value);
            }

            if (paymentMethodFilter.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.PaymentMethodId == paymentMethodFilter.Value);
            }

            if (yearFilter.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.TransactionDate.Year == yearFilter.Value);
                
                if (monthFilter.HasValue)
                {
                    expensesQuery = expensesQuery.Where(e => e.TransactionDate.Month == monthFilter.Value);
                }
            }

            // Wykonaj zapytanie
            var expenses = await expensesQuery
                .OrderByDescending(e => e.TransactionDate)
                .ToListAsync();

            // Przekaż dane do widoku
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name", categoryFilter);
            ViewData["PaymentMethods"] = new SelectList(paymentMethods, "PaymentMethodId", "Name", paymentMethodFilter);
            
            // Przygotuj listy lat i miesięcy
            var years = await _context.Expense
                .Where(e => e.UserId == currentUserId)
                .Select(e => e.TransactionDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            
            ViewData["Years"] = new SelectList(years, yearFilter);
            ViewData["Months"] = new SelectList(Enumerable.Range(1, 12).Select(m => 
                new { Value = m, Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) }),
                "Value", "Text", monthFilter);

            // Zapisz aktualne filtry
            ViewData["CurrentCategoryFilter"] = categoryFilter;
            ViewData["CurrentPaymentMethodFilter"] = paymentMethodFilter;
            ViewData["CurrentYearFilter"] = yearFilter;
            ViewData["CurrentMonthFilter"] = monthFilter;
            
            return View(expenses);
        }

        // GET: Expense/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(c => c.Category)
                .Include(c => c.PaymentMethod)
                .FirstOrDefaultAsync(m => m.ExpenseId == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expense/Create
        public IActionResult Create()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View();
        }

        // POST: Expense/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Amount,CategoryId,PaymentMethodId,TransactionDate,Description,Payee")] Expense expense)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (ModelState.IsValid)
            {
                expense.UserId = currentUserId;
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(expense);
        }

        // GET: Expense/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(expense);
        }

        // POST: Expense/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ExpenseId,UserId,Amount,CategoryId,PaymentMethodId,TransactionDate,Description,Payee")] Expense expense)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (id != expense.ExpenseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.ExpenseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(expense);
        }

        // GET: Expense/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.Category)
                .Include(e => e.PaymentMethod)
                .FirstOrDefaultAsync(e => e.ExpenseId == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(long id)
        {
            return _context.Expense.Any(e => e.ExpenseId == id);
        }

        private async void GetNavigationsProperties(long currentUserId)
        {
            var categories = await _context.Category
                .Where(c => c.UserId == currentUserId && c.Type == CategoryType.Expense)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            var paymentMethods = await _context.PaymentMethod
                .Where(p => p.UserId == currentUserId)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name");
            ViewData["PaymentMethodId"] = new SelectList(paymentMethods, "PaymentMethodId", "Name");

        }
    }
}
