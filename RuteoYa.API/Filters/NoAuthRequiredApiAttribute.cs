namespace RuteoYa.API.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class NoAuthRequiredApiAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Verifica si el usuario ya envió un token válido y está autenticado
        if (context.HttpContext.User.Identity != null && context.HttpContext.User.Identity.IsAuthenticated)
        {
            // Rechaza la petición porque el usuario ya tiene sesión activa
            context.Result = new BadRequestObjectResult(new { mensaje = "Ya te encuentras autenticado." });
        }

        base.OnActionExecuting(context);
    }
}
