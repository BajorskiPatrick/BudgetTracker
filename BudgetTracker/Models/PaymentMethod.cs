using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models;

public class PaymentMethod
{
    [Key]
    public long PaymentMethodId { get; set; }
    
    [Required]
    [Display(Name = "User")]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    [Display(Name = "User")]
    public virtual User? User { get; set; }

    [Required]
    [Display(Name = "Method name")]
    [MaxLength(50)]
    public required String Name { get; set; }
}