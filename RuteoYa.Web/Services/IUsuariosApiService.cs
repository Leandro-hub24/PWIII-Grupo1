using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

/// <summary>
/// Interfaz para el servicio que consume la API de usuarios.
/// </summary>
public interface IUsuariosApiService
{
    Task<List<UsuarioDto>> ObtenerTodosAsync();
    Task<UsuarioDto?> ObtenerPorIdAsync(int id);
    Task<(bool Success, string Message, UsuarioDto? Usuario)> CrearAsync(CrearUsuarioDto dto);
    Task<(bool Success, string Message)> ActualizarAsync(ActualizarUsuarioDto dto);
    Task<(bool Success, string Message)> EliminarAsync(int id);
    Task<(bool Success, UsuarioDto? Usuario)> ValidarCredencialesAsync(string email, string password);
}
