namespace RuteoYa.Entidades.DTOs;

/// <summary>
/// Resultado del cálculo de ruta óptima en auto entre dos puntos.
/// </summary>
public class DistanciaRutaDto
{
    public GeocoderDto Origen { get; set; } = new();
    public GeocoderDto Destino { get; set; } = new();

    /// <summary>Distancia de la ruta óptima en kilómetros.</summary>
    public double DistanciaKm { get; set; }

    /// <summary>Duración estimada en minutos.</summary>
    public int DuracionMinutos { get; set; }
}
