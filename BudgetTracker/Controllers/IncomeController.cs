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
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Income
        public async Task<IActionResult> Index(long? categoryFilter, int? yearFilter, int? monthFilter)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var categories = await _context.Category
                .Where(c => c.UserId == currentUserId && c.Type == CategoryType.Income)
                .OrderBy(c => c.Name)
                .ToListAsync();

            IQueryable<Income> incomesQuery = _context.Income
                .Include(e => e.Category)
                .Where(e => e.UserId == currentUserId);

            if (categoryFilter.HasValue)
            {
                incomesQuery = incomesQuery.Where(e => e.CategoryId == categoryFilter.Value);
            }

            if (yearFilter.HasValue)
            {
                incomesQuery = incomesQuery.Where(e => e.TransactionDate.Year == yearFilter.Value);
                
                if (monthFilter.HasValue)
                {
                    incomesQuery = incomesQuery.Where(e => e.TransactionDate.Month == monthFilter.Value);
                }
            }

            var incomes = await incomesQuery
                .OrderByDescending(e => e.TransactionDate)
                .ToListAsync();

            ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name", categoryFilter);
            
            var years = await _context.Income
                .Where(e => e.UserId == currentUserId)
                .Select(e => e.TransactionDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            
            ViewData["Years"] = new SelectList(years, yearFilter);
            ViewData["Months"] = new SelectList(Enumerable.Range(1, 12).Select(m => 
                new { Value = m, Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) }),
                "Value", "Text", monthFilter);

            ViewData["CurrentCategoryFilter"] = categoryFilter;
            ViewData["CurrentYearFilter"] = yearFilter;
            ViewData["CurrentMonthFilter"] = monthFilter;
            
            return View(incomes);
        }

        // GET: Income/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var income = await _context.Income
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.IncomeId == id);
            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        // GET: Income/Create
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

        // POST: Income/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Amount,TransactionDate,Description,Source")] Income income)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (ModelState.IsValid)
            {
                income.UserId = currentUserId;
                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(income);
        }

        // GET: Income/Edit/5
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

            var income = await _context.Income.FindAsync(id);
            if (income == null)
            {
                return NotFound();
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(income);
        }

        // POST: Income/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("IncomeId,UserId,CategoryId,Amount,TransactionDate,Description,Source")] Income income)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (id != income.IncomeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.IncomeId))
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
            
            return View(income);
        }

        // GET: Income/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var income = await _context.Income
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.IncomeId == id);
            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        // POST: Income/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var income = await _context.Income.FindAsync(id);
            if (income != null)
            {
                _context.Income.Remove(income);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncomeExists(long id)
        {
            return _context.Income.Any(e => e.IncomeId == id);
        }

        public async void GetNavigationsProperties(long currentUserId)
        {
            var categories = await _context.Category
                .Where(c => c.UserId == currentUserId && c.Type == CategoryType.Income)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name");
        }
    }
}
