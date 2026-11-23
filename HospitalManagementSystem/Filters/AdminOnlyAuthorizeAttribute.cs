using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalManagementSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AdminOnlyAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAdmin = context.HttpContext.Session.GetString("AdminAuthenticated") == "true"
                || string.Equals(context.HttpContext.Session.GetString("LoggedInUserRole"), "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new
                {
                    returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString
                });
            }
        }
    }
}
