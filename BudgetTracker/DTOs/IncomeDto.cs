namespace BudgetTracker.DTOs;

public class IncomeDto
{
    public long IncomeId { get; set; }
    public required long UserId { get; set; }
    public required long CategoryId { get; set; }
    public required Decimal Amount { get; set; }
    public required DateTime TransactionDate { get; set; }
    public String? Description { get; set; }
    public String? Source { get; set; }
}