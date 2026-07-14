using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace WebAdmin.Filters
{
    public class AdminAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("CurrentUserRole");

            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                context.Result = new RedirectResult("https://localhost:7089/Auth/Login");
            }

            base.OnActionExecuting(context);
        }
    }
}