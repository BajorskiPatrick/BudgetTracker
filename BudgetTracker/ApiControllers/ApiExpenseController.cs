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
    public class ApiExpenseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        private long GetCurrentUserId()
        {
            return (long)HttpContext.Items["CurrentUserId"]!;
        }

        // GET: api/ApiExpense
        // Możemy również obsłużyć filtry w API, podobnie jak w kontrolerach UI.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(
            [FromQuery] long? categoryId, 
            [FromQuery] long? paymentMethodId, 
            [FromQuery] int? year, 
            [FromQuery] int? month)
        {
            var userId = GetCurrentUserId();

            var expensesQuery = _context.Expense
                .Where(e => e.UserId == userId);

            if (categoryId.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.CategoryId == categoryId.Value);
            }
            if (paymentMethodId.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.PaymentMethodId == paymentMethodId.Value);
            }
            if (year.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.TransactionDate.Year == year.Value);
                if (month.HasValue)
                {
                    expensesQuery = expensesQuery.Where(e => e.TransactionDate.Month == month.Value);
                }
            }

            return await expensesQuery
                .OrderByDescending(e => e.TransactionDate)
                .Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    UserId = e.UserId,
                    Amount = e.Amount,
                    TransactionDate = e.TransactionDate,
                    Description = e.Description,
                    CategoryId = e.CategoryId,
                    PaymentMethodId = e.PaymentMethodId,
                    Payee = e.Payee,
                })
                .ToListAsync();
        }

        // GET: api/ApiExpense/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(long id)
        {
            var userId = GetCurrentUserId();
            var expense = await _context.Expense
                .Where(e => e.ExpenseId == id && e.UserId == userId)
                .Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    UserId = e.UserId,
                    Amount = e.Amount,
                    TransactionDate = e.TransactionDate,
                    Description = e.Description,
                    CategoryId = e.CategoryId,
                    PaymentMethodId = e.PaymentMethodId,
                    Payee = e.Payee,
                })
                .FirstOrDefaultAsync();

            if (expense == null)
            {
                return NotFound(new { Message = "Expense not found or you do not have access." });
            }

            return expense;
        }

        // POST: api/ApiExpense
        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> PostExpense([FromBody] Expense expense)
        {
            var userId = GetCurrentUserId();

            // Walidacja CategoryId i PaymentMethodId dla danego użytkownika
            if (!await _context.Category.AnyAsync(c => c.CategoryId == expense.CategoryId && c.UserId == userId && c.Type == CategoryType.Expense))
            {
                return BadRequest(new { Message = $"Category with ID {expense.CategoryId} not found or not an expense category for this user." });
            }
            if (!await _context.PaymentMethod.AnyAsync(pm => pm.PaymentMethodId == expense.PaymentMethodId && pm.UserId == userId))
            {
                return BadRequest(new { Message = $"Payment method with ID {expense.PaymentMethodId} not found for this user." });
            }

            expense.UserId = userId;

            _context.Expense.Add(expense);
            await _context.SaveChangesAsync();

            var expenseDto = new ExpenseDto
            {
                ExpenseId = expense.ExpenseId,
                UserId = expense.UserId,
                Amount = expense.Amount,
                TransactionDate = expense.TransactionDate,
                Description = expense.Description,
                CategoryId = expense.CategoryId,
                PaymentMethodId = expense.PaymentMethodId,
                Payee = expense.Payee
            };

            return CreatedAtAction(nameof(GetExpense), new { id = expense.ExpenseId }, expenseDto);
        }

        // PUT: api/ApiExpense/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(long id, [FromBody] Expense expense)
        {
            var userId = GetCurrentUserId();

            if (id != expense.ExpenseId)
            {
                return BadRequest(new { Message = "Expense ID mismatch." });
            }

            var existingExpense = await _context.Expense.AsNoTracking().FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);
            if (existingExpense == null)
            {
                return NotFound(new { Message = "Expense not found or you do not have access." });
            }

            // Walidacja CategoryId i PaymentMethodId dla danego użytkownika
            if (!await _context.Category.AnyAsync(c => c.CategoryId == expense.CategoryId && c.UserId == userId && c.Type == CategoryType.Expense))
            {
                return BadRequest(new { Message = $"Category with ID {expense.CategoryId} not found or not an expense category for this user." });
            }
            if (!await _context.PaymentMethod.AnyAsync(pm => pm.PaymentMethodId == expense.PaymentMethodId && pm.UserId == userId))
            {
                return BadRequest(new { Message = $"Payment method with ID {expense.PaymentMethodId} not found for this user." });
            }

            expense.UserId = userId;

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Expense.AnyAsync(e => e.ExpenseId == id && e.UserId == userId))
                {
                    return NotFound(new { Message = "Expense not found or already deleted." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ApiExpense/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(long id)
        {
            var userId = GetCurrentUserId();
            var expense = await _context.Expense.FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);
            if (expense == null)
            {
                return NotFound(new { Message = "Expense not found or you do not have access." });
            }

            _context.Expense.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}