using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace RuteoYa.Web.Common;

public class TokenDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Obtener el HttpContext actual del usuario en MVC
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null)
        {
            // 2. Extraer el token JWT guardado en los claims de la cookie local
            var token = httpContext.User?.FindFirst("JwtToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                // 3. Inyectar automáticamente el header Authorization en la petición
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 4. Continuar con el flujo normal de la petición HTTP hacia la API
        return await base.SendAsync(request, cancellationToken);
    }
}

