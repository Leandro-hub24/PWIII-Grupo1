using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

/// <summary>
/// Convierte una dirección textual en coordenadas geográficas (lat/lng).
/// </summary>
public interface IGeocoderService
{
    Task<GeocoderDto> GeocodeAsync(string direccion);
}
