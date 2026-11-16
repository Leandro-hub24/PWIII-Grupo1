using System.ComponentModel.DataAnnotations;
using RuteoYa.Entidades.Common.Enums;

namespace RuteoYa.Entidades.DTOs;

/// <summary>
/// Datos para publicar un envío (Pantalla 6 - Nuevo envío).
/// </summary>
public class CrearEnvioDto
{
    [Required(ErrorMessage = "El origen es obligatorio.")]
    [StringLength(200)]
    public string OrigenDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "El destino es obligatorio.")]
    [StringLength(200)]
    public string DestinoDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime Fecha { get; set; }

    [Range(0.01, 1000, ErrorMessage = "El peso debe ser mayor que cero.")]
    public decimal PesoKg { get; set; }

    [Required(ErrorMessage = "El tamaño es obligatorio.")]
    public TamanoEnvio? Tamano { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 10000000, ErrorMessage = "El monto ofrecido debe ser mayor que cero.")]
    public decimal Monto { get; set; }
}

/// <summary>
/// Envío que devuelve la API (Pantalla 7 - Mis envíos y detalle).
/// </summary>
public class EnvioDto
{
    public int Id { get; set; }
    public string OrigenDir { get; set; } = string.Empty;
    public string DestinoDir { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal PesoKg { get; set; }
    public TamanoEnvio Tamano { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public EstadoEnvio Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public bool SePuedeCancelar { get; set; }
    public DateTime CreadoEn { get; set; }

    // Viaje asociado (solo si un conductor lo aceptó)
    public int? ViajeId { get; set; }
    public string? ViajeOrigen { get; set; }
    public DateTime? FechaViaje { get; set; }

    public List<EnvioEstadoHistorialDto> Historial { get; set; } = new();
}

public class EnvioEstadoHistorialDto
{
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public class CambiarEstadoEnvioDto
{
    [Required]
    public EstadoEnvio? NuevoEstado { get; set; }
}
