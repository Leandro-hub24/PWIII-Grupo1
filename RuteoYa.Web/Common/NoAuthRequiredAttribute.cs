using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RuteoYa.Web.Common;

public class NoAuthRequiredAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Si el usuario ya inició sesión
        if (context.HttpContext.User.Identity != null && context.HttpContext.User.Identity.IsAuthenticated)
        {
            // Redirigirlo al Home (o al Dashboard) y frenar la ejecución
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }

        base.OnActionExecuting(context);
    }
}
