using RuteoYa.Entidades.DTOs;
using System.Net.Http.Json;

namespace RuteoYa.Web.Services;

/// <summary>
/// Servicio que consume la API REST de viajes vía HTTP.
/// </summary>
public class ViajesApiService : IViajesApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViajesApiService> _logger;

    public ViajesApiService(HttpClient httpClient, ILogger<ViajesApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DistanciaRutaDto?> CalcularDistanciaAsync(string origenDireccion, string destinoDireccion)
    {
        try
        {
            var request = new { OrigenDireccion = origenDireccion, DestinoDireccion = destinoDireccion };
            var response = await _httpClient.PostAsJsonAsync("api/viajes/calcular-distancia", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al calcular distancia. Status: {Status}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DistanciaRutaDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular distancia desde la API");
            return null;
        }
    }
}
