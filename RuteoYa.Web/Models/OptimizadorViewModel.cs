using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Models;

public class OptimizadorViewModel
{
    public int ViajeId { get; set; }
    public RestriccionesViajeDto Restricciones { get; set; } = new();
    public CombinacionDto? Recomendada { get; set; }
    public List<CombinacionDto> OtrasValidas { get; set; } = new();
    public List<CombinacionDto> Descartadas { get; set; } = new();

    public bool TieneSolucion => Recomendada != null && Recomendada.Envios.Any();

    // Porcentaje para las barras de Razor
    public int PorcentajePaquetes => Restricciones.MaxPaquetes > 0 && Recomendada != null
        ? (int)((decimal)Recomendada.CantidadPaquetes / Restricciones.MaxPaquetes * 100) : 0;

    public int PorcentajePeso => Restricciones.PesoMaxKg > 0 && Recomendada != null
        ? (int)(Recomendada.PesoTotalKg / Restricciones.PesoMaxKg * 100) : 0;

    public int PorcentajeDesvio => Restricciones.DesvioMaxKm > 0 && Recomendada != null
        ? (int)(Recomendada.DesvioTotalKm / Restricciones.DesvioMaxKm * 100) : 0;
    public List<ViajeResumenDto> ViajesActivos { get; set; } = new();
}

public class ConfirmarSeleccionViewModel
{
    public int ViajeId { get; set; }
    public string TramoViaje { get; set; } = string.Empty;
    public List<OportunidadDto> Envios { get; set; } = new();
    public decimal GananciaTotal { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal DesvioTotalKm { get; set; }
    public int CantidadPaquetes => Envios.Count;
    public string? ErrorConflicto { get; set; }
}