using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BudgetTracker.Utils;

public class AdminOnlyFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isAdmin = context.HttpContext.Session.GetString("IsAdmin");

        if (isAdmin == "False")
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        } 
    }
}