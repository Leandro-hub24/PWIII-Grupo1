using RuteoYa.Entidades;

namespace RuteoYa.Entidades.Interfaces.Repositories;

/// <summary>
/// Contrato de repositorio para operaciones de acceso a datos de usuarios.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<List<Usuario>> ObtenerTodosAsync();
    Task<Usuario> CrearAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
    Task EliminarAsync(int id);
    Task<bool> ExisteEmailAsync(string email);
}
