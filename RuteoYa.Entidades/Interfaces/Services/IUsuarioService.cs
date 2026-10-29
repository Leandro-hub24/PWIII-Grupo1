using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

/// <summary>
/// Contrato de servicio de lógica de negocio para usuarios.
/// </summary>
public interface IUsuarioService
{
    Task<UsuarioDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDto?> ObtenerPorEmailAsync(string email);
    Task<List<UsuarioDto>> ObtenerTodosAsync();
    Task<(bool Success, string Message, UsuarioDto? Usuario)> CrearAsync(CrearUsuarioDto dto);
    Task<(bool Success, string Message)> ActualizarAsync(ActualizarUsuarioDto dto);
    Task<(bool Success, string Message)> EliminarAsync(int id);
    Task<bool> ValidarCredencialesAsync(string email, string password);
}
