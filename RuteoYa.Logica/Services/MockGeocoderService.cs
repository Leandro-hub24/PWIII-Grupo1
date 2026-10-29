using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

/// <summary>
/// Implementación mock de geocodificación.
/// Devuelve coordenadas fijas hasta integrar una API real (Nominatim, Google Maps, etc.).
/// </summary>
public class MockGeocoderService : IGeocoderService
{
    // Coordenadas fijas centradas en Buenos Aires como placeholder
    private const decimal LatBase = -34.6037m;
    private const decimal LngBase = -58.3816m;

    public Task<GeocoderDto> GeocodeAsync(string direccion)
    {
        // TODO: reemplazar con llamada real a Nominatim o Google Geocoding API
        var resultado = new GeocoderDto
        {
            Direccion = direccion,
            Lat = LatBase,
            Lng = LngBase
        };

        return Task.FromResult(resultado);
    }
}
