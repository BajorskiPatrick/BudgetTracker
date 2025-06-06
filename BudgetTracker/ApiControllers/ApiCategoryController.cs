using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Utils;
using BudgetTracker.DTOs;

namespace BudgetTracker.ApiControllers;

// [ApiController] oznacza, że kontroler jest kontrolerem API i automatycznie obsługuje rzeczy takie jak
// inferowanie źródeł parametrów, walidację ModelState oraz formatowanie odpowiedzi JSON.
[ApiController]
// [Route("api/[controller]")] definiuje bazową ścieżkę dla tego kontrolera, np. /api/category.
[Route("api/[controller]")]
// [ServiceFilter(typeof(ApiAuthorizationFilter))] Aplikuje nasz niestandardowy filtr autoryzacji API
// do wszystkich akcji w tym kontrolerze.
[ServiceFilter(typeof(ApiAuthorizationFilter))]
public class ApiCategoryController : ControllerBase // Kontrolery API dziedziczą po ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiCategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Pomocnicza metoda do pobierania UserId z HttpContext.Items
    // Po co? Filtr ApiAuthorizationFilter zapisuje tam UserId po udanej autoryzacji.
    private long GetCurrentUserId()
    {
        return (long)HttpContext.Items["CurrentUserId"]!;
    }

    // GET: api/ApiCategory
    // Dlaczego HttpGet? Mapuje to do żądań GET.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var userId = GetCurrentUserId();
        // Pobieramy tylko kategorie należące do zalogowanego użytkownika.
        return await _context.Category
            .Where(c => c.UserId == userId)
            .Select(c => new CategoryDto // Mapujemy na DTO
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Type = c.Type,
                Description = c.Description
            })
            .ToListAsync();
    }

    // GET: api/ApiCategory/5
    // Dlaczego HttpGet("{id}")? Mapuje to do żądań GET z ID w ścieżce, np. /api/category/5.
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(long id)
    {
        var userId = GetCurrentUserId();
        // Szukamy konkretnej kategorii, upewniając się, że należy do użytkownika.
        var category = await _context.Category
            .Where(c => c.CategoryId == id && c.UserId == userId)
            .Select(c => new CategoryDto // Mapujemy na DTO
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Type = c.Type,
                Description = c.Description
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            // Dlaczego NotFound? Jeśli zasób nie istnieje lub użytkownik nie ma do niego dostępu.
            return NotFound(new { Message = "Category not found or you do not have access." });
        }

        return category;
    }

    // POST: api/ApiCategory
    // Dlaczego HttpPost? Mapuje to do żądań POST, używanych do tworzenia nowych zasobów.
    // Dlaczego FromBody? Parametr category będzie deserializowany z ciała żądania JSON.
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> PostCategory([FromBody] Category category)
    {
        var userId = GetCurrentUserId();

        // 1. Walidacja ModelState (automatycznie przez [ApiController])
        // Jeśli model Category ma błędy walidacji (np. wymagane pola),
        // [ApiController] automatycznie zwróci 400 Bad Request z detalami.

        // 2. Ustawienie UserId
        // Upewniamy się, że nowa kategoria zostanie przypisana do uwierzytelnionego użytkownika.
        category.UserId = userId;

        // 3. Sprawdzenie unikalności nazwy kategorii dla użytkownika
        // ModelBuilder ma Unique Index, ale lepiej jest to sprawdzić wcześniej
        // i zwrócić bardziej konkretny błąd niż generyczny błąd bazy danych.
        var existingCategory = await _context.Category
            .AnyAsync(c => c.UserId == userId && c.Name == category.Name);
        if (existingCategory)
        {
            return BadRequest(new { Message = "A category with this name already exists for this user." });
        }
        
        _context.Category.Add(category);
        await _context.SaveChangesAsync();
        
        var categoryDto = new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Type = category.Type,
            Description = category.Description
        };

        // Dlaczego CreatedAtAction? Zwraca 201 Created z nagłówkiem Location
        // wskazującym URI nowo utworzonego zasobu. To standard dla REST po POST.
        return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, categoryDto);
    }

    // PUT: api/ApiCategory/5
    // Dlaczego HttpPut("{id}")? Mapuje do żądań PUT, używanych do aktualizacji istniejących zasobów.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory(long id, [FromBody] Category category)
    {
        var userId = GetCurrentUserId();

        // 1. Walidacja ID z URL i ID w ciele żądania
        if (id != category.CategoryId)
        {
            // Dlaczego BadRequest? Niezgodność ID jest błędem po stronie klienta.
            return BadRequest(new { Message = "Category ID mismatch." });
        }

        // 2. Upewnienie się, że użytkownik jest właścicielem zasobu
        var existingCategory = await _context.Category.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
        if (existingCategory == null)
        {
            return NotFound(new { Message = "Category not found or you do not have access." });
        }

        // 3. Upewnienie się, że UserId w obiekcie PUT jest prawidłowy (nie może być zmieniony)
        category.UserId = userId; // Nadpisz na wszelki wypadek, aby użytkownik nie mógł zmienić właściciela

        // 4. Walidacja ModelState
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Zwróci szczegółowe błędy walidacji
        }
        
        // 5. Sprawdzenie unikalności nazwy kategorii dla użytkownika (jeśli zmieniona)
        var nameConflict = await _context.Category
            .AnyAsync(c => c.UserId == userId && c.Name == category.Name && c.CategoryId != id);
        if (nameConflict)
        {
            return BadRequest(new { Message = "A category with this name already exists for this user." });
        }

        _context.Entry(category).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Sprawdzenie, czy zasób nadal istnieje i czy użytkownik ma do niego dostęp.
            if (!await _context.Category.AnyAsync(c => c.CategoryId == id && c.UserId == userId))
            {
                return NotFound(new { Message = "Category not found or already deleted." });
            }
            else
            {
                throw; // Rzuć wyjątek, jeśli to prawdziwy problem z konkurencją.
            }
        }

        // Dlaczego NoContent? Standard dla udanej aktualizacji, która nie zwraca żadnej treści.
        return NoContent();
    }

    // DELETE: api/ApiCategory/5
    // Dlaczego HttpDelete("{id}")? Mapuje do żądań DELETE, używanych do usuwania zasobów.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var userId = GetCurrentUserId();
        // Szukamy kategorii do usunięcia, upewniając się, że należy do użytkownika.
        var category = await _context.Category.FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
        if (category == null)
        {
            return NotFound(new { Message = "Category not found or you do not have access." });
        }

        // Przed usunięciem kategorii, należy upewnić się, że nie jest ona używana
        // w żadnych wydatkach ani przychodach. W przeciwnym razie baza danych może
        // zwrócić błąd klucza obcego.
        var hasExpenses = await _context.Expense.AnyAsync(e => e.CategoryId == id);
        var hasIncomes = await _context.Income.AnyAsync(i => i.CategoryId == id);
        var hasLimits = await _context.Limit.AnyAsync(l => l.CategoryId == id);

        if (hasExpenses || hasIncomes || hasLimits)
        {
            return BadRequest(new { Message = "Category cannot be deleted because it is associated with existing expenses, incomes, or limits." });
        }

        _context.Category.Remove(category);
        await _context.SaveChangesAsync();

        // Dlaczego NoContent? Standard dla udanego usunięcia.
        return NoContent();
    }
}