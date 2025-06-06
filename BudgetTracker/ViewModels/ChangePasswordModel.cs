using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels;

public class ChangePasswordModel
{
    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [Display(Name = "Password")]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password")] // Porównuje z polem Password
    public required string ConfirmPassword { get; set; } 
}