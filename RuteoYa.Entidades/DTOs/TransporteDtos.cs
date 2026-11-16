using RuteoYa.Entidades.Common.Enums;

namespace RuteoYa.Entidades.DTOs;

public class TransporteDto
{
    public int EnvioId { get; set; }
    public int ViajeId { get; set; }
    public string? ViajeOrigen { get; set; }
    public string? ViajeDestino { get; set; }
    public DateTime FechaViaje { get; set; }
    public string Descripcion { get; set; } = "";
    public string OrigenDir { get; set; } = "";
    public string DestinoDir { get; set; } = "";
    public decimal Ganancia { get; set; }
    public EstadoEnvio Estado { get; set; }
    public string EstadoNombre { get; set; } = "";

    /// <summary>Próximo estado que puede marcar el conductor (null si no hay acción).</summary>
    public EstadoEnvio? EstadoSiguiente { get; set; }
}