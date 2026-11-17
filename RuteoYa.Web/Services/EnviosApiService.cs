using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

public interface IEnviosApiService
{
    Task<List<EnvioDto>> GetMisEnviosAsync(EstadoEnvio? estado);
    Task<EnvioDto?> GetEnvioAsync(int id);
    /// <summary>Devuelve el envío creado o el mensaje de error de la API.</summary>
    Task<(EnvioDto? Envio, string? Error)> CrearEnvioAsync(CrearEnvioDto dto);
    Task<string?> CancelarEnvioAsync(int id);
}

/// <summary>
/// Consume /api/envios. El MVC nunca toca el DbContext: todo pasa por la API.
/// Usa el cliente "MiApi", que agrega el JWT del usuario (TokenDelegatingHandler).
/// </summary>
public class EnviosApiService : IEnviosApiService
{
    private readonly HttpClient _http;

    public EnviosApiService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("MiApi");
    }

    public async Task<List<EnvioDto>> GetMisEnviosAsync(EstadoEnvio? estado)
    {
        var url = estado.HasValue ? $"api/envios?estado={(byte)estado.Value}" : "api/envios";
        return await _http.GetFromJsonAsync<List<EnvioDto>>(url) ?? new List<EnvioDto>();
    }

    public async Task<EnvioDto?> GetEnvioAsync(int id)
    {
        var response = await _http.GetAsync($"api/envios/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EnvioDto>();
    }

    public async Task<(EnvioDto? Envio, string? Error)> CrearEnvioAsync(CrearEnvioDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/envios", dto);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<EnvioDto>(), null);

        return (null, await LeerError(response));
    }

    public async Task<string?> CancelarEnvioAsync(int id)
    {
        var response = await _http.PutAsync($"api/envios/{id}/cancelar", null);
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
