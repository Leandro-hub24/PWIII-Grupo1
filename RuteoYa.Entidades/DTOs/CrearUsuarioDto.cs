namespace RuteoYa.Entidades.DTOs;

/// <summary>
/// DTO para crear un nuevo usuario.
/// </summary>
public class CrearUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
