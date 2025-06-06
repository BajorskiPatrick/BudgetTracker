using System.Globalization;
using System.Text.Json.Serialization;
using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Utils;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Dodajemy niestandardowy filtr autoryzacji API do kontenera DI
// Scope (zakres) oznacza, że nowa instancja filtra będzie tworzona dla każdego żądania HTTP.
builder.Services.AddScoped<ApiAuthorizationFilter>();

// W Program.cs (dla .NET 6+)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguracja sesji
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

await SeedInitialUser(app);

var anInvariantBasedCulture = new CultureInfo("en-US");

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(anInvariantBasedCulture), // Ustaw domyślną kulturę dla żądań
    SupportedCultures = new List<CultureInfo> { anInvariantBasedCulture }, // Lista wspieranych kultur
    SupportedUICultures = new List<CultureInfo> { anInvariantBasedCulture } // Lista wspieranych kultur dla UI (np. zasobów językowych)
};

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();


async Task SeedInitialUser(IApplicationBuilder appBuilder)
{
    using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
    {
        var services = serviceScope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            if (!await dbContext.User.AnyAsync(u => u.IsAdmin == true))
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "pbajorski@student.agh.edu.pl",
                    Name = "Patrick",
                    Surname = "Bajorski",
                    PasswordHash = HashEngine.ComputeMd5Hash("admin"),
                    RegistrationDate = DateTime.Now,
                    ApiToken = ApiTokenProvider.Generate(),
                    IsAdmin = true,
                };

                dbContext.User.Add(adminUser);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("First user (admin) created successfully.");
            }
            else
            {
                Console.WriteLine("Database already contains users. No initial user created.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}