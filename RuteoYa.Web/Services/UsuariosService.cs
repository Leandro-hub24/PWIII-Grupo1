
using RuteoYa.Entidades.Entities;
using RuteoYa.Web.Models;
using System;
using System.Net;
using System.Net.Http.Json;

namespace RuteoYa.Web.Services;

public interface IUsuariosService
{
    Task<Usuario?> GetUsuarioByIdAsync();
    Task<bool> CreateUsuarioAsync(RegisterViewModel model);
}
public class UsuariosService : IUsuariosService
{
    private readonly HttpClient _httpClient;
    public UsuariosService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MiApi");
    }

    public async Task<Usuario?> GetUsuarioByIdAsync()
    {
        return await _httpClient.GetFromJsonAsync<Usuario?>($"api/usuarios/mi-cuenta");
        
    }

    public async Task<bool> CreateUsuarioAsync(RegisterViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/usuarios/create", model);
        if (!response.IsSuccessStatusCode)
        {
            // Lee el error exacto que devuelve la API para saber qué falló
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error de API: {response.StatusCode} - {errorContent}");
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception("No se pudo crear el usuario. Verificá los datos e intentá nuevamente.");
            }
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocurrió un error inesperado al realizar el registro. Intente más tarde.");
            }
        }
        return await response.Content.ReadFromJsonAsync<bool>();
    }

}
