namespace BudgetTracker.DTOs;

public class LimitDto
{
    public long BudgetId { get; set; }
    public required long UserId { get; set; }
    public required long CategoryId { get; set; }
    public required Decimal Amount { get; set; }
}