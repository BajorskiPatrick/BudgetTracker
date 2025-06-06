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
    public class LimitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LimitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Amount
        public async Task<IActionResult> Index()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var limits = await _context.Limit
                .Include(l => l.Category)
                .Where(l => l.UserId == currentUserId)
                .ToListAsync();
            
            return View(limits);
        }

        // GET: Amount/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var limits = await _context.Limit
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.BudgetId == id);
            if (limits == null)
            {
                return NotFound();
            }

            return View(limits);
        }

        // GET: Amount/Create
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

        // POST: Amount/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Amount")] Limit limit)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (ModelState.IsValid)
            {
                limit.UserId = currentUserId;
                _context.Add(limit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(limit);
        }

        // GET: Amount/Edit/5
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

            var limits = await _context.Limit.FindAsync(id);
            if (limits == null)
            {
                return NotFound();
            }
            
            GetNavigationsProperties(currentUserId);
            
            return View(limits);
        }

        // POST: Amount/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("BudgetId,UserId,CategoryId,Amount")] Limit limit)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (id != limit.BudgetId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(limit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LimitsExists(limit.BudgetId))
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
            
            return View(limit);
        }

        // GET: Amount/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var limits = await _context.Limit
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.BudgetId == id);
            if (limits == null)
            {
                return NotFound();
            }

            return View(limits);
        }

        // POST: Amount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var limits = await _context.Limit.FindAsync(id);
            if (limits != null)
            {
                _context.Limit.Remove(limits);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LimitsExists(long id)
        {
            return _context.Limit.Any(e => e.BudgetId == id);
        }

        public async void GetNavigationsProperties(long currentUserId)
        {
            var exp_categories = await _context.Category
                .Where(c => c.UserId == currentUserId && c.Type == CategoryType.Expense)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            var categoryIdsWithLimits = await _context.Limit
                .Where(l => l.UserId == currentUserId)
                .Select(l => l.CategoryId)
                .ToListAsync();
            
            var availableCategories = exp_categories
                .Where(c => !categoryIdsWithLimits.Contains(c.CategoryId))
                .ToList();
            
            ViewData["CategoryId"] = new SelectList(availableCategories, "CategoryId", "Name");
        }
    }
}
