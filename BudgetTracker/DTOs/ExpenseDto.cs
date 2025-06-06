namespace BudgetTracker.DTOs;

public class ExpenseDto
{
    public long ExpenseId { get; set; }
    public required long UserId { get; set; }
    public required Decimal Amount { get; set; }
    public required DateTime TransactionDate { get; set; }
    public String? Description { get; set; }
    public required long CategoryId { get; set; }
    public long? PaymentMethodId { get; set; }
    public String? Payee { get; set; }
}