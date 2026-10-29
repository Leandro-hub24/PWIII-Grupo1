using RuteoYa.Entidades;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _repository.ObtenerPorIdAsync(id);
        return usuario != null ? MapearADto(usuario) : null;
    }

    public async Task<UsuarioDto?> ObtenerPorEmailAsync(string email)
    {
        var usuario = await _repository.ObtenerPorEmailAsync(email);
        return usuario != null ? MapearADto(usuario) : null;
    }

    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        var usuarios = await _repository.ObtenerTodosAsync();
        return usuarios.Select(MapearADto).ToList();
    }

    public async Task<(bool Success, string Message, UsuarioDto? Usuario)> CrearAsync(CrearUsuarioDto dto)
    {
        // Validar email único
        if (await _repository.ExisteEmailAsync(dto.Email))
        {
            return (false, "Ya existe un usuario con ese email", null);
        }

        // Hashear password usando BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            PasswordHash = passwordHash,
            CreadoEn = DateTime.UtcNow
        };

        var creado = await _repository.CrearAsync(usuario);
        return (true, "Usuario creado exitosamente", MapearADto(creado));
    }

    public async Task<(bool Success, string Message)> ActualizarAsync(ActualizarUsuarioDto dto)
    {
        var usuario = await _repository.ObtenerPorIdAsync(dto.Id);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        // Validar email único (excepto el propio usuario)
        var existeEmail = await _repository.ObtenerPorEmailAsync(dto.Email);
        if (existeEmail != null && existeEmail.Id != dto.Id)
        {
            return (false, "Ya existe otro usuario con ese email");
        }

        usuario.Nombre = dto.Nombre;
        usuario.Apellido = dto.Apellido;
        usuario.Email = dto.Email;

        await _repository.ActualizarAsync(usuario);
        return (true, "Usuario actualizado exitosamente");
    }

    public async Task<(bool Success, string Message)> EliminarAsync(int id)
    {
        var usuario = await _repository.ObtenerPorIdAsync(id);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        await _repository.EliminarAsync(id);
        return (true, "Usuario eliminado exitosamente");
    }

    public async Task<bool> ValidarCredencialesAsync(string email, string password)
    {
        var usuario = await _repository.ObtenerPorEmailAsync(email);
        if (usuario == null)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
    }

    private UsuarioDto MapearADto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            CreadoEn = usuario.CreadoEn
        };
    }
}
