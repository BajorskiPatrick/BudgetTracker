using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Utils;
using BudgetTracker.DTOs;

namespace BudgetTracker.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiAuthorizationFilter))]
    public class ApiLimitController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiLimitController(ApplicationDbContext context)
        {
            _context = context;
        }

        private long GetCurrentUserId()
        {
            return (long)HttpContext.Items["CurrentUserId"]!;
        }

        // GET: api/ApiLimit
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LimitDto>>> GetLimits()
        {
            var userId = GetCurrentUserId();
            return await _context.Limit
                .Where(l => l.UserId == userId)
                .Select(l => new LimitDto
                {
                    BudgetId = l.BudgetId,
                    UserId = l.UserId,
                    CategoryId = l.CategoryId,
                    Amount = l.Amount
                })
                .ToListAsync();
        }

        // GET: api/ApiLimit/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LimitDto>> GetLimit(long id)
        {
            var userId = GetCurrentUserId();
            var limit = await _context.Limit
                .Where(l => l.BudgetId == id && l.UserId == userId)
                .Select(l => new LimitDto
                {
                    BudgetId = l.BudgetId,
                    UserId = l.UserId,
                    CategoryId = l.CategoryId,
                    Amount = l.Amount
                })
                .FirstOrDefaultAsync();

            if (limit == null)
            {
                return NotFound(new { Message = "Limit not found or you do not have access." });
            }

            return limit;
        }

        // POST: api/ApiLimit
        [HttpPost]
        public async Task<ActionResult<LimitDto>> PostLimit([FromBody] Limit limit)
        {
            var userId = GetCurrentUserId();

            // Walidacja CategoryId dla danego użytkownika i typu Expense
            if (!await _context.Category.AnyAsync(c => c.CategoryId == limit.CategoryId && c.UserId == userId && c.Type == CategoryType.Expense))
            {
                return BadRequest(new { Message = $"Category with ID {limit.CategoryId} not found or not an expense category for this user." });
            }
            
            // Sprawdzenie, czy limit dla tej kategorii już istnieje (unique index)
            if (await _context.Limit.AnyAsync(l => l.UserId == userId && l.CategoryId == limit.CategoryId))
            {
                return BadRequest(new { Message = "A limit for this category already exists for this user. Use PUT to update it." });
            }

            limit.UserId = userId;
            _context.Limit.Add(limit);
            await _context.SaveChangesAsync();

            var limitDto = new LimitDto
            {
                BudgetId = limit.BudgetId,
                UserId = limit.UserId,
                CategoryId = limit.CategoryId,
                Amount = limit.Amount
            };
            
            return CreatedAtAction(nameof(GetLimit), new { id = limit.BudgetId }, limitDto);
        }

        // PUT: api/ApiLimit/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLimit(long id, [FromBody] Limit limit)
        {
            var userId = GetCurrentUserId();

            if (id != limit.BudgetId)
            {
                return BadRequest(new { Message = "Limit ID mismatch." });
            }

            var existingLimit = await _context.Limit.AsNoTracking().FirstOrDefaultAsync(l => l.BudgetId == id && l.UserId == userId);
            if (existingLimit == null)
            {
                return NotFound(new { Message = "Limit not found or you do not have access." });
            }
            
            // Walidacja CategoryId dla danego użytkownika i typu Expense
            if (!await _context.Category.AnyAsync(c => c.CategoryId == limit.CategoryId && c.UserId == userId && c.Type == CategoryType.Expense))
            {
                return BadRequest(new { Message = $"Category with ID {limit.CategoryId} not found or not an expense category for this user." });
            }

            // Sprawdzenie, czy limit dla tej kategorii już istnieje (unique index) i nie jest to ten sam limit
            if (await _context.Limit.AnyAsync(l => l.UserId == userId && l.CategoryId == limit.CategoryId && l.BudgetId != id))
            {
                return BadRequest(new { Message = "A limit for this category already exists for this user. Cannot change category to one that already has a limit." });
            }

            limit.UserId = userId;
            _context.Entry(limit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Limit.AnyAsync(l => l.BudgetId == id && l.UserId == userId))
                {
                    return NotFound(new { Message = "Limit not found or already deleted." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ApiLimit/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLimit(long id)
        {
            var userId = GetCurrentUserId();
            var limit = await _context.Limit.FirstOrDefaultAsync(l => l.BudgetId == id && l.UserId == userId);
            if (limit == null)
            {
                return NotFound(new { Message = "Limit not found or you do not have access." });
            }

            _context.Limit.Remove(limit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}