using BudgetTracker.Models;

namespace BudgetTracker.DTOs;

public class CategoryDto
{
    public long CategoryId { get; set; }
    public required string Name { get; set; }
    public required CategoryType Type { get; set; } // CategoryType jest enumem, więc to jest ok
    public string? Description { get; set; }
}