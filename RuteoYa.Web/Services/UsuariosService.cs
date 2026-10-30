
using RuteoYa.Web.Models;
using System;
using System.Net.Http.Json;

namespace RuteoYa.Web.Services;

public interface IUsuariosService
{
    Task<LoginViewModel?> GetUsuarioByIdAsync(int id);
    Task<bool> CreateUsuarioAsync(RegisterViewModel model);
}
public class UsuariosService : IUsuariosService
{
    private readonly HttpClient _httpClient;
    public UsuariosService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MiApi");
    }

    public async Task<LoginViewModel?> GetUsuarioByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<LoginViewModel?>($"api/usuarios/{id}");
        
    }

    public async Task<bool> CreateUsuarioAsync(RegisterViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/usuarios/create", model);
        if (!response.IsSuccessStatusCode)
        {
            // Lee el error exacto que devuelve la API para saber qué falló
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error de API: {response.StatusCode} - {errorContent}");
            return false;
        }
        return await response.Content.ReadFromJsonAsync<bool>();
    }

}
