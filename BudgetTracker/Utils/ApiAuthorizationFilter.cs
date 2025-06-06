using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BudgetTracker.Data;
using Microsoft.EntityFrameworkCore; 

namespace BudgetTracker.Utils;

public class ApiAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly ApplicationDbContext _context;

    // Dlaczego IAsyncAuthorizationFilter i wstrzykiwanie ApplicationDbContext?
    // Potrzebujemy asynchronicznego dostępu do bazy danych (_context)
    // aby wyszukać użytkownika i zweryfikować token.
    // Standardowy IAuthorizationFilter jest synchroniczny, a operacje na DB powinny być async.
    public ApiAuthorizationFilter(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. Pobranie nagłówków autoryzacyjnych
        // Nagłówki HTTP są standardowym miejscem do przekazywania tokenów API.
        // Używamy niestandardowych nagłówków 'X-Username' i 'X-Api-Token' dla czytelności.
        var username = context.HttpContext.Request.Headers["X-Username"].FirstOrDefault();
        var apiToken = context.HttpContext.Request.Headers["X-Api-Token"].FirstOrDefault();

        // 2. Walidacja obecności nagłówków
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(apiToken))
        {
            // Jeśli brakuje danych uwierzytelniających, zwracamy 401 Unauthorized.
            // Zwracamy JsonResult z komunikatem, aby klient API wiedział, co się stało.
            context.Result = new UnauthorizedObjectResult(new { Message = "Authentication credentials missing (X-Username and/or X-Api-Token headers required)." });
            return;
        }

        // 3. Wyszukanie użytkownika w bazie danych
        // Asynchroniczne wyszukiwanie użytkownika po nazwie.
        var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

        // 4. Walidacja użytkownika i tokenu
        if (user == null || user.ApiToken != apiToken)
        {
            // Jeśli użytkownik nie istnieje lub token jest nieprawidłowy, zwracamy 401 Unauthorized.
            context.Result = new UnauthorizedObjectResult(new { Message = "Invalid username or API token." });
            return;
        }

        // 5. Przekazanie UserId do kontekstu żądania
        // Jeśli autoryzacja przebiegła pomyślnie, zapisujemy UserId w HttpContext.Items.
        // To pozwala kontrolerom API na łatwy dostęp do ID uwierzytelnionego użytkownika
        // bez ponownego wyszukiwania go w bazie danych.
        context.HttpContext.Items["CurrentUserId"] = user.UserId;
    }
}