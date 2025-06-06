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
    public class ApiIncomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiIncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private long GetCurrentUserId()
        {
            return (long)HttpContext.Items["CurrentUserId"]!;
        }

        // GET: api/ApiIncome
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetIncomes(
            [FromQuery] long? categoryId, 
            [FromQuery] int? year, 
            [FromQuery] int? month)
        {
            var userId = GetCurrentUserId();

            var incomesQuery = _context.Income
                .Where(i => i.UserId == userId);

            if (categoryId.HasValue)
            {
                incomesQuery = incomesQuery.Where(i => i.CategoryId == categoryId.Value);
            }
            if (year.HasValue)
            {
                incomesQuery = incomesQuery.Where(i => i.TransactionDate.Year == year.Value);
                if (month.HasValue)
                {
                    incomesQuery = incomesQuery.Where(i => i.TransactionDate.Month == month.Value);
                }
            }

            return await incomesQuery
                .OrderByDescending(i => i.TransactionDate)
                .Select(i => new IncomeDto
                {
                    IncomeId = i.IncomeId,
                    UserId = i.UserId,
                    CategoryId = i.CategoryId,
                    Amount = i.Amount,
                    TransactionDate = i.TransactionDate,
                    Description = i.Description,
                    Source = i.Source
                })
                .ToListAsync();
        }

        // GET: api/ApiIncome/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeDto>> GetIncome(long id)
        {
            var userId = GetCurrentUserId();
            var income = await _context.Income
                .Where(i => i.IncomeId == id && i.UserId == userId)
                .Select(i => new IncomeDto
                {
                    IncomeId = i.IncomeId,
                    UserId = i.UserId,
                    CategoryId = i.CategoryId,
                    Amount = i.Amount,
                    TransactionDate = i.TransactionDate,
                    Description = i.Description,
                    Source = i.Source
                })
                .FirstOrDefaultAsync();

            if (income == null)
            {
                return NotFound(new { Message = "Income not found or you do not have access." });
            }

            return income;
        }

        // POST: api/ApiIncome
        [HttpPost]
        public async Task<ActionResult<IncomeDto>> PostIncome([FromBody] Income income)
        {
            var userId = GetCurrentUserId();

            // Walidacja CategoryId dla danego użytkownika
            if (!await _context.Category.AnyAsync(c => c.CategoryId == income.CategoryId && c.UserId == userId && c.Type == CategoryType.Income))
            {
                return BadRequest(new { Message = $"Category with ID {income.CategoryId} not found or not an income category for this user." });
            }

            income.UserId = userId;

            _context.Income.Add(income);
            await _context.SaveChangesAsync();

            var incomeDto = new IncomeDto
            {
                IncomeId = income.IncomeId,
                UserId = income.UserId,
                CategoryId = income.CategoryId,
                Amount = income.Amount,
                TransactionDate = income.TransactionDate,
                Description = income.Description,
                Source = income.Source
            };
            
            return CreatedAtAction(nameof(GetIncome), new { id = income.IncomeId }, incomeDto);
        }

        // PUT: api/ApiIncome/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIncome(long id, [FromBody] Income income)
        {
            var userId = GetCurrentUserId();

            if (id != income.IncomeId)
            {
                return BadRequest(new { Message = "Income ID mismatch." });
            }

            var existingIncome = await _context.Income.AsNoTracking().FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);
            if (existingIncome == null)
            {
                return NotFound(new { Message = "Income not found or you do not have access." });
            }

            // Walidacja CategoryId dla danego użytkownika
            if (!await _context.Category.AnyAsync(c => c.CategoryId == income.CategoryId && c.UserId == userId && c.Type == CategoryType.Income))
            {
                return BadRequest(new { Message = $"Category with ID {income.CategoryId} not found or not an income category for this user." });
            }

            income.UserId = userId;

            _context.Entry(income).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Income.AnyAsync(i => i.IncomeId == id && i.UserId == userId))
                {
                    return NotFound(new { Message = "Income not found or already deleted." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ApiIncome/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncome(long id)
        {
            var userId = GetCurrentUserId();
            var income = await _context.Income.FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);
            if (income == null)
            {
                return NotFound(new { Message = "Income not found or you do not have access." });
            }

            _context.Income.Remove(income);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}