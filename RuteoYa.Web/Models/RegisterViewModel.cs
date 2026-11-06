using System.ComponentModel.DataAnnotations;

namespace RuteoYa.Web.Models;

public class RegisterViewModel
{
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
    [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
    [Compare("PasswordHash", ErrorMessage = "Las contraseñas no coinciden.")]
    public string PasswordConfirmed { get; set; }
}
