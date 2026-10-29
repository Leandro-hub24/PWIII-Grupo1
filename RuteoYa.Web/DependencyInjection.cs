using Microsoft.Extensions.DependencyInjection;
using RuteoYa.Web.Logic;
using RuteoYa.Web.Services;

namespace RuteoYa.Web;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios propios de la capa Web (orquestadores, servicios HTTP, etc.).
    /// </summary>
    public static IServiceCollection AddWebServices(this IServiceCollection services, string apiBaseUrl)
    {
        // Servicios HTTP que consumen la API
        services.AddHttpClient<IUsuariosApiService, UsuariosApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Orquestadores de lógica de presentación
        services.AddScoped<ViajesOrchestrator>();
        services.AddScoped<UsuariosOrchestrator>();

        return services;
    }
}
