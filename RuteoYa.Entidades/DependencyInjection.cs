using Microsoft.Extensions.DependencyInjection;

namespace RuteoYa.Entidades;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios del núcleo (Core/Entidades).
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Registrar servicios de Core aquí cuando se implementen
        // Ejemplo: services.AddScoped<IViajesService, ViajesService>();

        return services;
    }
}
