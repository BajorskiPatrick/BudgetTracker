using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Utils;

namespace BudgetTracker.Controllers
{
    [TypeFilter(typeof(AuthorizationFilter))]
    [TypeFilter(typeof(UserOnlyFilter))]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index(CategoryType? typeFilter = null)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Rozpocznij od podstawowego zapytania
            var categoriesQuery = _context.Category
                .Where(c => c.UserId == currentUserId);
    
            // Zastosuj filtr jeśli został podany
            if (typeFilter.HasValue)
            {
                categoriesQuery = categoriesQuery.Where(c => c.Type == typeFilter.Value);
            }
    
            // Posortuj i wykonaj zapytanie
            var userCategories = await categoriesQuery
                .OrderBy(c => c.Name)
                .ToListAsync();
    
            // Przekaż aktualny filtr do widoku
            ViewData["CurrentFilter"] = typeFilter;
            
            return View(userCategories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,Description")] Category category)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (ModelState.IsValid)
            {
                category.UserId = currentUserId;
                
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
                    {
                        ModelState.AddModelError(nameof(category.Name), "Kategoria o tej nazwie już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas zapisywania kategorii. Spróbuj ponownie.");
                    }
                }
            }
            
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("CategoryId,UserId,Name,Type,Description")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
                    {
                        ModelState.AddModelError(nameof(category.Name), "Kategoria o tej nazwie już istnieje.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas zapisywania kategorii. Spróbuj ponownie.");
                    }
                }
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
                _context.Category.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(long id)
        {
            return _context.Category.Any(e => e.CategoryId == id);
        }
    }
}
