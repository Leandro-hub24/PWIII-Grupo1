using System.ComponentModel.DataAnnotations;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Models;

public class ViajeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "El origen es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Origen")]
    public string OrigenDir { get; set; } = "";

    [Required(ErrorMessage = "El destino es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Destino")]
    public string DestinoDir { get; set; } = "";

    [Required(ErrorMessage = "La hora de salida es obligatoria.")]
    [Display(Name = "Hora de salida")]
    public TimeOnly? HoraSalida { get; set; }

    [Required(ErrorMessage = "La hora de llegada es obligatoria.")]
    [Display(Name = "Hora de llegada")]
    public TimeOnly? HoraLlegada { get; set; }

    [Required(ErrorMessage = "La cantidad de paquetes es obligatoria.")]
    [Range(1, 100, ErrorMessage = "Debe ser entre 1 y 100 paquetes.")]
    [Display(Name = "Máximo de paquetes")]
    public int? MaxPaquetes { get; set; }

    [Required(ErrorMessage = "El peso máximo es obligatorio.")]
    [Range(1, 5000, ErrorMessage = "El peso debe ser entre 1 y 5000 kg.")]
    [Display(Name = "Peso máximo (kg)")]
    public int? PesoMaxKg { get; set; }

    [Range(0, 100, ErrorMessage = "El desvío debe ser entre 0 y 100 km.")]
    [Display(Name = "Desvío máximo (km)")]
    public int DesvioMaxKm { get; set; } = 5;

    public List<DayOfWeek> Dias { get; set; } = new List<DayOfWeek>();

    public List<string> Paradas { get; set; } = new List<string>();

    public CrearViajeDto ADto()
    {
        var dto = new CrearViajeDto();
        dto.OrigenDir = OrigenDir;
        dto.DestinoDir = DestinoDir;
        dto.HoraSalida = HoraSalida ?? default;
        dto.HoraLlegada = HoraLlegada ?? default;
        dto.MaxPaquetes = MaxPaquetes ?? 0;
        dto.PesoMaxKg = PesoMaxKg ?? 0;
        dto.DesvioMaxKm = DesvioMaxKm;
        dto.Dias = Dias;

        int orden = 1;
        foreach (var parada in Paradas)
        {
            if (!string.IsNullOrWhiteSpace(parada))
            {
                dto.Paradas.Add(new CrearParadaDto { Direccion = parada.Trim(), Orden = orden });
                orden++;
            }
        }
        return dto;
    }

    public static ViajeFormViewModel DesdeDto(ViajeDto viaje)
    {
        var model = new ViajeFormViewModel();
        model.Id = viaje.Id;
        model.OrigenDir = viaje.OrigenDir;
        model.DestinoDir = viaje.DestinoDir;
        model.HoraSalida = viaje.HoraSalida;
        model.HoraLlegada = viaje.HoraLlegada;
        model.MaxPaquetes = viaje.MaxPaquetes;
        model.PesoMaxKg = (int)viaje.PesoMaxKg;
        model.DesvioMaxKm = (int)viaje.DesvioMaxKm;
        model.Dias = viaje.Dias;
        model.Paradas = viaje.Paradas.OrderBy(p => p.Orden).Select(p => p.Direccion).ToList();
        return model;
    }
}

public static class Dias
{
    public static string[] Nombres = { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };

    public static DayOfWeek[] LunesADomingo =
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    };

    public static string Texto(List<DayOfWeek> dias)
    {
        return string.Join(", ", dias.Select(d => Nombres[(int)d]));
    }
}
