using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels;

public class NewUserViewModel
{
    [Required]
    [Display(Name = "Username")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public required string Email { get; set; }
    
    [Required]
    [Display(Name = "Name")]
    public required String Name { get; set; }
    
    [Required]
    [Display(Name = "Surname")]
    public required String Surname { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [Display(Name = "Password")]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password")]
    public required String ConfirmPassword { get; set; }
    
    [Display(Name="Admin?")]
    public Boolean IsAdmin { get; set; }
}