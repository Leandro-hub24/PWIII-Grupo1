using RuteoYa.Entidades.DTOs;
using System.Net.Http.Json;

namespace RuteoYa.Web.Services;

/// <summary>
/// Servicio que consume la API REST de usuarios vía HTTP.
/// </summary>
public class UsuariosApiService : IUsuariosApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsuariosApiService> _logger;

    public UsuariosApiService(HttpClient httpClient, ILogger<UsuariosApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        try
        {
            var usuarios = await _httpClient.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios");
            return usuarios ?? new List<UsuarioDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios desde la API");
            return new List<UsuarioDto>();
        }
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/usuarios/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UsuarioDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {Id} desde la API", id);
            return null;
        }
    }

    public async Task<(bool Success, string Message, UsuarioDto? Usuario)> CrearAsync(CrearUsuarioDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/usuarios", dto);

            if (response.IsSuccessStatusCode)
            {
                var usuario = await response.Content.ReadFromJsonAsync<UsuarioDto>();
                return (true, "Usuario creado exitosamente", usuario);
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Error al crear usuario", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario en la API");
            return (false, "Error de conexión con el servidor", null);
        }
    }

    public async Task<(bool Success, string Message)> ActualizarAsync(ActualizarUsuarioDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/usuarios/{dto.Id}", dto);

            if (response.IsSuccessStatusCode)
                return (true, "Usuario actualizado exitosamente");

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Error al actualizar usuario");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {Id} en la API", dto.Id);
            return (false, "Error de conexión con el servidor");
        }
    }

    public async Task<(bool Success, string Message)> EliminarAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/usuarios/{id}");

            if (response.IsSuccessStatusCode)
                return (true, "Usuario eliminado exitosamente");

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, error?.Message ?? "Error al eliminar usuario");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {Id} en la API", id);
            return (false, "Error de conexión con el servidor");
        }
    }

    public async Task<(bool Success, UsuarioDto? Usuario)> ValidarCredencialesAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/usuarios/validar", request);

            if (response.IsSuccessStatusCode)
            {
                var usuario = await response.Content.ReadFromJsonAsync<UsuarioDto>();
                return (true, usuario);
            }

            return (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar credenciales en la API");
            return (false, null);
        }
    }

    private record ErrorResponse(string Message);
}
