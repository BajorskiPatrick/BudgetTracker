using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models;

public class Category
{
    [Key]
    public long CategoryId { get; set; }
    
    [Required]
    [Display(Name="User ID")]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    [Display(Name="User")]
    public virtual User? User { get; set; }
    
    [Required]
    [Display(Name="Category name")]
    [MaxLength(50)]
    public required String Name { get; set; }
    
    [Required]
    [Display(Name="Category type")]
    public required CategoryType Type { get; set; }
    
    [Display(Name="Category description")]
    [DisplayFormat(NullDisplayText = "None")]
    [MaxLength(200)]
    public String? Description { get; set; }
}