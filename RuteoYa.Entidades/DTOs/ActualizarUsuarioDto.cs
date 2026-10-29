namespace RuteoYa.Entidades.DTOs;

/// <summary>
/// DTO para actualizar datos de un usuario existente.
/// </summary>
public class ActualizarUsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
