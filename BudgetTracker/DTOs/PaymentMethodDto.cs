namespace BudgetTracker.DTOs;

public class PaymentMethodDto
{
    public long PaymentMethodId { get; set; }
    public required long UserId { get; set; }
    public required String Name { get; set; }
}