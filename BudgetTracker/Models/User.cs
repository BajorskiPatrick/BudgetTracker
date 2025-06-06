using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models;

public class User
{
    [Key]
    public long UserId { get; set; }
    
    [Required]
    [Display(Name="Username")]
    [MaxLength(50)]
    public required String Username { get; set; }
    
    [Required]
    [EmailAddress]
    [Display(Name="Email")]
    [MaxLength(80)]
    public required String Email { get; set; }
    
    [Required]
    [Display(Name="Name")]
    [MaxLength(50)]
    public required String Name { get; set; }
    
    [Required]
    [Display(Name="Surname")]
    [MaxLength(50)]
    public required String Surname { get; set; }
    
    [Required]
    [Display(Name="Admin?")]
    public required Boolean IsAdmin { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    [Display(Name="Registration date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime RegistrationDate { get; set; }
    
    [Required]
    [StringLength(32)]
    public required String PasswordHash { get; set; }
    
    [Required]
    [StringLength(32)]
    public required String? ApiToken { get; set; }
    
    [DisplayFormat(NullDisplayText = "None")]
    public virtual ICollection<Category>? Categories { get; set; }
    
    [DisplayFormat(NullDisplayText = "None")]
    public virtual ICollection<Expense>? Expenses { get; set; }
    
    [DisplayFormat(NullDisplayText = "None")]
    public virtual ICollection<Income>? Incomes { get; set; }
    
    [DisplayFormat(NullDisplayText = "None")]
    public virtual ICollection<PaymentMethod>? PaymentMethods { get; set; }
    
    [DisplayFormat(NullDisplayText = "None")]
    public virtual ICollection<Limit>? Limits { get; set; }
}