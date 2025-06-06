using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models;

public class Expense
{
    [Key]
    public long ExpenseId { get; set; }
    
    [Required]
    [Display(Name="User ID")]
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    [Display(Name="User")]
    public virtual User? User { get; set; }
    
    [Required]
    [Display(Name="Amount")]
    public Decimal Amount { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    [Display(Name="Transaction date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime TransactionDate { get; set; }
    
    [Display(Name="Description")]
    [DisplayFormat(NullDisplayText = "None")]
    [MaxLength(200)]
    public String? Description { get; set; }
    
    [Required]
    [Display(Name="Category")]
    public long CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    [Display(Name="Category")]
    [DisplayFormat(NullDisplayText = "None")]
    public virtual Category? Category { get; set; }
    
    [Required]
    [Display(Name="Payment Method")]
    public long? PaymentMethodId { get; set; }
    
    [ForeignKey("PaymentMethodId")]
    [Display(Name="Payment Method")]
    [DisplayFormat(NullDisplayText = "None")]
    public virtual PaymentMethod? PaymentMethod { get; set; }
    
    [Display(Name="Payee")]
    [DisplayFormat(NullDisplayText = "None")]
    [MaxLength(50)]
    public String? Payee { get; set; }
}