using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BudgetTracker.Utils;

public class UserOnlyFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isAdmin = context.HttpContext.Session.GetString("IsAdmin");

        if (isAdmin == "True")
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}