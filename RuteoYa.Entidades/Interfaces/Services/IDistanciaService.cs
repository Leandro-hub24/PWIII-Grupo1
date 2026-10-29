using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

/// <summary>
/// Calcula la distancia y duración de la ruta óptima en auto entre dos direcciones.
/// </summary>
public interface IDistanciaService
{
    /// <summary>
    /// Recibe dos direcciones en texto, las geocodifica y devuelve
    /// la distancia y duración de la ruta óptima en auto.
    /// </summary>
    Task<DistanciaRutaDto> CalcularAsync(string origenDireccion, string destinoDireccion);
}
