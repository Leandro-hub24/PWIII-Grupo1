using Microsoft.Extensions.DependencyInjection;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Repositories;
using RuteoYa.Logica.Services;

namespace RuteoYa.Logica;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los repositorios y servicios de infraestructura.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Servicios de negocio
        services.AddScoped<IUsuarioService, UsuarioService>();

        // Servicios de mapas/distancia (mock hasta integrar APIs reales)
        services.AddScoped<IGeocoderService, MockGeocoderService>();
        services.AddScoped<IDistanciaService, MockDistanciaService>();

        return services;
    }
}
