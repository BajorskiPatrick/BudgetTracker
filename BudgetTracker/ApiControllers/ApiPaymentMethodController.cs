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
    public class ApiPaymentMethodController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiPaymentMethodController(ApplicationDbContext context)
        {
            _context = context;
        }

        private long GetCurrentUserId()
        {
            return (long)HttpContext.Items["CurrentUserId"]!;
        }

        // GET: api/ApiPaymentMethod
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetPaymentMethods()
        {
            var userId = GetCurrentUserId();
            return await _context.PaymentMethod
                .Where(pm => pm.UserId == userId)
                .Select(pm => new PaymentMethodDto
                {
                    PaymentMethodId = pm.PaymentMethodId,
                    UserId = pm.UserId,
                    Name = pm.Name
                })
                .ToListAsync();
        }

        // GET: api/ApiPaymentMethod/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethod(long id)
        {
            var userId = GetCurrentUserId();
            var paymentMethod = await _context.PaymentMethod
                .Where(pm => pm.PaymentMethodId == id && pm.UserId == userId)
                .Select(pm => new PaymentMethodDto
                {
                    PaymentMethodId = pm.PaymentMethodId,
                    UserId = pm.UserId,
                    Name = pm.Name
                })
                .FirstOrDefaultAsync();

            if (paymentMethod == null)
            {
                return NotFound(new { Message = "Payment method not found or you do not have access." });
            }

            return paymentMethod;
        }

        // POST: api/ApiPaymentMethod
        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> PostPaymentMethod([FromBody] PaymentMethod paymentMethod)
        {
            var userId = GetCurrentUserId();
            paymentMethod.UserId = userId;

            // Sprawdzenie unikalności nazwy metody płatności dla użytkownika
            var existingPaymentMethod = await _context.PaymentMethod
                .AnyAsync(pm => pm.UserId == userId && pm.Name == paymentMethod.Name);
            if (existingPaymentMethod)
            {
                return BadRequest(new { Message = "A payment method with this name already exists for this user." });
            }

            _context.PaymentMethod.Add(paymentMethod);
            await _context.SaveChangesAsync();

            var paymentMethodDto = new PaymentMethodDto
            {
                PaymentMethodId = paymentMethod.PaymentMethodId,
                UserId = paymentMethod.UserId,
                Name = paymentMethod.Name
            };

            return CreatedAtAction(nameof(GetPaymentMethod), new { id = paymentMethod.PaymentMethodId }, paymentMethodDto);
        }

        // PUT: api/ApiPaymentMethod/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentMethod(long id, [FromBody] PaymentMethod paymentMethod)
        {
            var userId = GetCurrentUserId();

            if (id != paymentMethod.PaymentMethodId)
            {
                return BadRequest(new { Message = "Payment method ID mismatch." });
            }

            var existingPaymentMethod = await _context.PaymentMethod.AsNoTracking().FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && pm.UserId == userId);
            if (existingPaymentMethod == null)
            {
                return NotFound(new { Message = "Payment method not found or you do not have access." });
            }

            paymentMethod.UserId = userId; // Nadpisz na wszelki wypadek

            // Sprawdzenie unikalności nazwy metody płatności dla użytkownika (jeśli zmieniona)
            var nameConflict = await _context.PaymentMethod
                .AnyAsync(pm => pm.UserId == userId && pm.Name == paymentMethod.Name && pm.PaymentMethodId != id);
            if (nameConflict)
            {
                return BadRequest(new { Message = "A payment method with this name already exists for this user." });
            }

            _context.Entry(paymentMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.PaymentMethod.AnyAsync(pm => pm.PaymentMethodId == id && pm.UserId == userId))
                {
                    return NotFound(new { Message = "Payment method not found or already deleted." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ApiPaymentMethod/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(long id)
        {
            var userId = GetCurrentUserId();
            var paymentMethod = await _context.PaymentMethod.FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && pm.UserId == userId);
            if (paymentMethod == null)
            {
                return NotFound(new { Message = "Payment method not found or you do not have access." });
            }
            
            // Przed usunięciem metody płatności, należy upewnić się, że nie jest ona używana
            // w żadnych wydatkach. W przeciwnym razie baza danych może zwrócić błąd klucza obcego.
            var hasExpenses = await _context.Expense.AnyAsync(e => e.PaymentMethodId == id);
            if (hasExpenses)
            {
                return BadRequest(new { Message = "Payment method cannot be deleted because it is associated with existing expenses." });
            }

            _context.PaymentMethod.Remove(paymentMethod);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}