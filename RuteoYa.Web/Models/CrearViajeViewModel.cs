using System.ComponentModel.DataAnnotations;

namespace RuteoYa.Web.Models;

/// <summary>
/// ViewModel específico de la capa Web para crear viajes.
/// Contiene validaciones y lógica de presentación.
/// </summary>
public class CrearViajeViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "La dirección de origen es obligatoria")]
    [StringLength(500)]
    public string OrigenDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección de destino es obligatoria")]
    [StringLength(500)]
    public string DestinoDir { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Hora de salida")]
    public TimeOnly HoraSalida { get; set; }

    [Required]
    [Display(Name = "Hora de llegada")]
    public TimeOnly HoraLlegada { get; set; }

    [Range(1, 100, ErrorMessage = "Debe aceptar entre 1 y 100 paquetes")]
    [Display(Name = "Máximo de paquetes")]
    public int MaxPaquetes { get; set; }

    [Range(0.1, 5000, ErrorMessage = "El peso debe ser entre 0.1 y 5000 kg")]
    [Display(Name = "Peso máximo (kg)")]
    public decimal PesoMaxKg { get; set; }

    [Range(0, 100, ErrorMessage = "El desvío debe ser entre 0 y 100 km")]
    [Display(Name = "Desvío máximo (km)")]
    public decimal DesvioMaxKm { get; set; }
}
