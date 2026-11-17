using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

public interface IViajesApiService
{
    Task<List<ViajeDto>> GetMisViajesAsync(bool? activo);
    Task<ViajeDto?> GetViajeAsync(int id);
    Task<(ViajeDto? Viaje, string? Error)> CrearViajeAsync(CrearViajeDto dto);
    Task<(ViajeDto? Viaje, string? Error)> ActualizarViajeAsync(int id, CrearViajeDto dto);
    Task<string?> DesactivarViajeAsync(int id);
    Task<string?> ActivarViajeAsync(int id);
    Task<(RutaViajeDto? Ruta, string? Error)> CalcularRutaAsync(CalcularRutaDto dto);
}

public class ViajesApiService : IViajesApiService
{
    private readonly HttpClient _http;

    public ViajesApiService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("MiApi");
    }

    public async Task<List<ViajeDto>> GetMisViajesAsync(bool? activo)
    {
        var url = "api/viajes";
        if (activo == true) url = "api/viajes?activo=true";
        if (activo == false) url = "api/viajes?activo=false";

        var viajes = await _http.GetFromJsonAsync<List<ViajeDto>>(url);
        return viajes ?? new List<ViajeDto>();
    }

    public async Task<ViajeDto?> GetViajeAsync(int id)
    {
        var response = await _http.GetAsync("api/viajes/" + id);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ViajeDto>();
    }

    public async Task<(ViajeDto? Viaje, string? Error)> CrearViajeAsync(CrearViajeDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/viajes", dto);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ViajeDto>(), null);
        return (null, await LeerError(response));
    }

    public async Task<(ViajeDto? Viaje, string? Error)> ActualizarViajeAsync(int id, CrearViajeDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/viajes/" + id, dto);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ViajeDto>(), null);
        return (null, await LeerError(response));
    }

    public async Task<string?> DesactivarViajeAsync(int id)
    {
        var response = await _http.PutAsync("api/viajes/" + id + "/desactivar", null);
        if (response.IsSuccessStatusCode) return null;
        return await LeerError(response);
    }

    public async Task<string?> ActivarViajeAsync(int id)
    {
        var response = await _http.PutAsync("api/viajes/" + id + "/activar", null);
        if (response.IsSuccessStatusCode) return null;
        return await LeerError(response);
    }

    public async Task<(RutaViajeDto? Ruta, string? Error)> CalcularRutaAsync(CalcularRutaDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/viajes/calcular-ruta", dto);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<RutaViajeDto>(), null);
        return (null, await LeerError(response));
    }

    private static async Task<string> LeerError(HttpResponseMessage response)
    {
        try
        {
            var texto = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(texto);
            if (json.RootElement.TryGetProperty("mensaje", out var mensaje))
                return mensaje.GetString() ?? "Error.";
        }
        catch (JsonException)
        {
        }
        return "Error de la API (" + (int)response.StatusCode + ").";
    }
}
