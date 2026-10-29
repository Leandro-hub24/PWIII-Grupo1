using System.ComponentModel.DataAnnotations;

namespace RuteoYa.Web.Models;

/// <summary>
/// ViewModel para editar datos de un usuario existente.
/// </summary>
public class EditarUsuarioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}
