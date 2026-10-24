using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalManagementSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAdmin = context.HttpContext.Session.GetString("AdminAuthenticated") == "true";
            var isStaff = !string.IsNullOrWhiteSpace(context.HttpContext.Session.GetString("LoggedInUserRole"));
            if (!isAdmin && !isStaff)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new
                {
                    returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString
                });
            }
        }
    }
}
