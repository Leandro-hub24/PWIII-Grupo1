using System.Net.Http.Json;
using System.Text.Json;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

public interface ITransportesApiService
{
    Task<List<TransporteDto>> GetMisTransportesAsync(EstadoEnvio? estado);
    /// <summary>Devuelve null si salió bien o el mensaje de error de la API.</summary>
    Task<string?> CambiarEstadoAsync(int envioId, EstadoEnvio nuevo);
}

/// <summary>
/// Consume /api/transportes (Pantalla 12 - Mis transportes).
/// Usa el cliente "MiApi", que agrega el JWT del usuario.
/// </summary>
public class TransportesApiService : ITransportesApiService
{
    private readonly HttpClient _http;

    public TransportesApiService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("MiApi");
    }

    public async Task<List<TransporteDto>> GetMisTransportesAsync(EstadoEnvio? estado)
    {
        var url = estado.HasValue ? $"api/transportes?estado={(byte)estado.Value}" : "api/transportes";
        return await _http.GetFromJsonAsync<List<TransporteDto>>(url) ?? new List<TransporteDto>();
    }

    public async Task<string?> CambiarEstadoAsync(int envioId, EstadoEnvio nuevo)
    {
        var accion = nuevo switch
        {
            EstadoEnvio.Retirado => "retirar",
            EstadoEnvio.EnCamino => "en-camino",
            EstadoEnvio.Entregado => "entregar",
            _ => null
        };
        if (accion == null) return "Estado no permitido para un transporte.";

        var response = await _http.PutAsync($"api/transportes/{envioId}/{accion}", null);
        return response.IsSuccessStatusCode ? null : await LeerError(response);
    }

    private static async Task<string> LeerError(HttpResponseMessage response)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("mensaje", out var m)) return m.GetString() ?? "Error.";
            if (doc.RootElement.TryGetProperty("title", out var t)) return t.GetString() ?? "Error.";
        }
        catch (JsonException) { }
        return $"Error de la API ({(int)response.StatusCode}).";
    }
}