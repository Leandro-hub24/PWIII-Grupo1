using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Web.Services;

public class OportunidadesApiService : IOportunidadesApiService
{

    private readonly HttpClient _httpClient;
    private readonly ILogger<OportunidadesApiService> _logger;

    public OportunidadesApiService(HttpClient httpClient, ILogger<OportunidadesApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Envio>> ObtenerEnviosCompatiblesAsync(int viajeId, string? orden)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Envio>>($"api/oportunidades/compatibles?viajeId={viajeId}&orden={orden}") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener oportunidades compatibles");
            return new();
        }
    }

    public async Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int envioId, int viajeId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DetalleOportunidadDto>($"api/oportunidades/detalle/{envioId}?viajeId={viajeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el detalle de la oportunidad");
            return null;
        }
    }

    public async Task<(bool Success, string Message)> AceptarEnvioAsync(int envioId, int viajeId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/oportunidades/aceptar", new { EnvioId = envioId, ViajeId = viajeId });
            if (response.IsSuccessStatusCode)
            {
                return (true, "Envío aceptado correctamente");
            }
            return (false, "El envío ya no se encuentra disponible.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aceptar el envío");
            return (false, "Error de conexión con el servidor");
        }
    }




}
