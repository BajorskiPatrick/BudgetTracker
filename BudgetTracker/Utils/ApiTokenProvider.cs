namespace BudgetTracker.Utils;

public class ApiTokenProvider
{
    public static String Generate()
    {
        return Guid.NewGuid().ToString("N");
    }
}