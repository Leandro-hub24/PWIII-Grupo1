using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;

namespace RuteoYa.API;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios propios de la capa API (autenticación JWT, Swagger, etc.).
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Ejemplo: services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...
        // Ejemplo: services.AddSwaggerGen(options => { ... });
        services.AddHttpClient<IGeocoderService, GeocoderService>();
        return services;
    }
}
