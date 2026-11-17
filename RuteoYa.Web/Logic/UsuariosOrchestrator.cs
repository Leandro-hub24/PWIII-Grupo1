using RuteoYa.Entidades.DTOs;
using RuteoYa.Web.Models;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Logic;

/// <summary>
/// Orquestador de lógica de presentación para Usuarios.
/// Coordina llamadas a la API y transforma datos entre ViewModels y DTOs.
/// </summary>
public class UsuariosOrchestrator
{
    private readonly IUsuariosApiService _apiService;

    public UsuariosOrchestrator(IUsuariosApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Registra un nuevo usuario validando las reglas de UI.
    /// </summary>
    public async Task<(bool Success, string Message, int? UsuarioId)> RegistrarAsync(RegistroUsuarioViewModel model)
    {
        try
        {
            var dto = new CrearUsuarioDto
            {
                Nombre = model.Nombre.Trim(),
                Apellido = model.Apellido.Trim(),
                Email = model.Email.Trim().ToLower(),
                Password = model.Password
            };

            var resultado = await _apiService.CrearAsync(dto);

            if (resultado.Success && resultado.Usuario != null)
            {
                return (true, resultado.Message, resultado.Usuario.Id);
            }

            return (false, resultado.Message, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar usuario: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Valida credenciales de login.
    /// </summary>
    public async Task<(bool Success, string Message, UsuarioDto? Usuario)> LoginAsync(LoginViewModel model)
    {
        try
        {
            var resultado = await _apiService.ValidarCredencialesAsync(model.Email, model.Password);

            if (!resultado.Success)
            {
                return (false, "Email o contraseña incorrectos", null);
            }

            return (true, "Login exitoso", resultado.Usuario);
        }
        catch (Exception ex)
        {
            return (false, $"Error al iniciar sesión: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Actualiza los datos de un usuario.
    /// </summary>
    public async Task<(bool Success, string Message)> ActualizarAsync(EditarUsuarioViewModel model)
    {
        try
        {
            var dto = new ActualizarUsuarioDto
            {
                Id = model.Id,
                Nombre = model.Nombre.Trim(),
                Apellido = model.Apellido.Trim(),
                Email = model.Email.Trim().ToLower()
            };

            return await _apiService.ActualizarAsync(dto);
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar usuario: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene todos los usuarios para listar.
    /// </summary>
    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        return await _apiService.ObtenerTodosAsync();
    }

    /// <summary>
    /// Obtiene un usuario por ID para edición.
    /// </summary>
    public async Task<EditarUsuarioViewModel?> ObtenerParaEditarAsync(int id)
    {
        var usuario = await _apiService.ObtenerPorIdAsync(id);
        if (usuario == null)
            return null;

        return new EditarUsuarioViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email
        };
    }

    /// <summary>
    /// Elimina un usuario.
    /// </summary>
    public async Task<(bool Success, string Message)> EliminarAsync(int id)
    {
        return await _apiService.EliminarAsync(id);
    }
}
