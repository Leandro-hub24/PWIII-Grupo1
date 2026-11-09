using System.ComponentModel.DataAnnotations;

namespace RuteoYa.Entidades.DTOs;

public class CrearViajeDto
{
    [Required(ErrorMessage = "El origen es obligatorio.")]
    [StringLength(200)]
    public string OrigenDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "El destino es obligatorio.")]
    [StringLength(200)]
    public string DestinoDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "La hora de salida es obligatoria.")]
    public TimeOnly HoraSalida { get; set; }

    [Required(ErrorMessage = "La hora de llegada es obligatoria.")]
    public TimeOnly HoraLlegada { get; set; }

    [Range(1, 100, ErrorMessage = "Debe aceptar entre 1 y 100 paquetes.")]
    public int MaxPaquetes { get; set; }

    [Range(0.1, 5000, ErrorMessage = "El peso debe ser entre 0.1 y 5000 kg.")]
    public decimal PesoMaxKg { get; set; }

    [Range(0, 100, ErrorMessage = "El desvío debe ser entre 0 y 100 km.")]
    public decimal DesvioMaxKm { get; set; }

    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un día.")]
    public List<DayOfWeek> Dias { get; set; } = new();

    public List<CrearParadaDto> Paradas { get; set; } = new();
}

public class CrearParadaDto
{
    [Required(ErrorMessage = "La dirección de la parada es obligatoria.")]
    [StringLength(200)]
    public string Direccion { get; set; } = string.Empty;

    [Range(1, 50)]
    public int Orden { get; set; }
}

public class ViajeDto
{
    public int Id { get; set; }
    public string OrigenDir { get; set; } = string.Empty;
    public decimal OrigenLat { get; set; }
    public decimal OrigenLng { get; set; }
    public string DestinoDir { get; set; } = string.Empty;
    public decimal DestinoLat { get; set; }
    public decimal DestinoLng { get; set; }
    public TimeOnly HoraSalida { get; set; }
    public TimeOnly HoraLlegada { get; set; }
    public int MaxPaquetes { get; set; }
    public decimal PesoMaxKg { get; set; }
    public decimal DesvioMaxKm { get; set; }
    public int DistanciaKm { get; set; }
    public bool Activo { get; set; }
    public List<DayOfWeek> Dias { get; set; } = new();
    public List<ParadaDto> Paradas { get; set; } = new();
}

public class ParadaDto
{
    public int Id { get; set; }
    public int Orden { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}
