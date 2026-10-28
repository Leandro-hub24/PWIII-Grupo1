using System.ComponentModel.DataAnnotations;

namespace RuteoYa.Entidades;

public partial class Usuario
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50)]
    public string Nombre { get; set; }
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50)]
    public string Apellido { get; set; }
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "La contraseña debe tener al menos 8 caracteres, incluyendo una letra mayúscula, una letra minúscula, un número y un carácter especial.")]
    public string PasswordHash { get; set; }
    public DateTime CreadoEn { get; set; }

    public int ViajeId { get; set; } 
    public List<Viaje> Viajes { get; set; } = new List<Viaje>();
    public int EnvioId { get; set; }
    public List<Envio> Envios { get; set; } = new List<Envio>();
}
