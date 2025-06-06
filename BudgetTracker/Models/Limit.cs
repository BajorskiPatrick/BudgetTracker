using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models;

public class Limit
{
    [Key]
    public long BudgetId { get; set; }
    
    [Required]
    [Display(Name = "User")]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    [Display(Name = "User")]
    public virtual User? User { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    public long CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    [Display(Name = "Category")]
    public virtual Category? Category { get; set; }
    
    [Required]
    [Display(Name="Expenses limit")]
    public Decimal Amount { get; set; }
}