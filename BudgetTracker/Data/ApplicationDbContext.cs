using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> User { get; set; } = default!;
    public DbSet<Expense> Expense { get; set; } = default!;
    public DbSet<Income> Income { get; set; } = default!;
    public DbSet<Category> Category { get; set; } = default!;
    public DbSet<PaymentMethod> PaymentMethod { get; set; } = default!;
    public DbSet<Limit> Limit { get; set; } = default!;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Username}).IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email}).IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => new { c.UserId, c.Name }).IsUnique();
        
        modelBuilder.Entity<PaymentMethod>()
            .HasIndex(p => new { p.UserId, p.Name }).IsUnique();

        modelBuilder.Entity<Limit>()
            .HasIndex(m => new { m.UserId, m.CategoryId }).IsUnique();
        
        modelBuilder.Entity<Category>()
            .Property(e => e.Type).HasConversion<string>();
    }
}