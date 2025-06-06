using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BudgetTracker.Utils;

public class AuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isLoggedIn = context.HttpContext.Session.GetString("IsLoggedIn");
        
        if (string.IsNullOrEmpty(isLoggedIn) || isLoggedIn != "True")
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}