namespace RuteoYa.Entidades.DTOs;

/// <summary>
/// DTO para transferir datos de usuario entre capas (sin password).
/// </summary>
public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; }
    
    public string NombreCompleto => $"{Nombre} {Apellido}";
}
