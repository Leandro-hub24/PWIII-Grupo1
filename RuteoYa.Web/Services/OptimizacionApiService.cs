using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

public class OptimizacionApiService : IOptimizacionApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<OptimizacionApiService> _logger;

    public OptimizacionApiService(IHttpClientFactory httpClientFactory, ILogger<OptimizacionApiService> logger)
    {
        _http = httpClientFactory.CreateClient("MiApi");
        _logger = logger;
    }

    public async Task<(ResultadoOptimizacionDto? Data, string? Error)> ObtenerOptimizacionAsync(int viajeId)
    {
        try
        {
            var response = await _http.GetAsync($"api/optimizacion/{viajeId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return (null, $"No se encontró un viaje activo #{viajeId} para tu usuario.");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, "Tu sesión expiró o no estás autenticado.");

            if (!response.IsSuccessStatusCode)
                return (null, await LeerErrorAsync(response));

            var data = await response.Content.ReadFromJsonAsync<ResultadoOptimizacionDto>();
            return (data, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener optimización del viaje {ViajeId}", viajeId);
            return (null, "No se pudo conectar con el servicio de optimización.");
        }
    }

    public async Task<(bool Success, string Message)> ConfirmarCombinacionAsync(int viajeId, List<int> envioIds)
    {
        try
        {
            var dto = new ConfirmarAsignacionDto
            {
                ViajeId = viajeId,
                EnvioIds = envioIds
            };

            var response = await _http.PostAsJsonAsync("api/optimizacion/confirmar", dto);

            if (response.IsSuccessStatusCode)
                return (true, "Combinación reservada con éxito.");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (false, "Tu sesión expiró o no estás autenticado.");

            if (response.StatusCode == HttpStatusCode.Conflict)
                return (false, "Uno o más paquetes ya no se encuentran disponibles. Se recalculó la selección.");

            return (false, await LeerErrorAsync(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar la combinación del viaje {ViajeId}", viajeId);
            return (false, "Error de conexión con el servidor.");
        }
    }

    public async Task<List<ViajeResumenDto>> ObtenerViajesActivosAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/optimizacion/mis-viajes");
            if (!response.IsSuccessStatusCode)
                return new List<ViajeResumenDto>();

            return await response.Content.ReadFromJsonAsync<List<ViajeResumenDto>>() ?? new List<ViajeResumenDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los viajes activos del usuario");
            return new List<ViajeResumenDto>();
        }
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage response)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("mensaje", out var m))
                return m.GetString() ?? "Error.";
            if (doc.RootElement.TryGetProperty("title", out var t))
                return t.GetString() ?? "Error.";
        }
        catch (JsonException) { }

        return $"Error de la API ({(int)response.StatusCode}).";
    }
}