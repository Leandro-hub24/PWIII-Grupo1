using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

/// <summary>
/// Implementación mock del cálculo de distancia.
/// Geocodifica ambas direcciones y devuelve valores fijos hasta integrar
/// una API real de rutas (OSRM, Google Directions, etc.).
/// </summary>
public class MockDistanciaService : IDistanciaService
{
    private readonly IGeocoderService _geocoder;

    // Valores mock: distancia y duración fijos hasta tener API real
    private const double DistanciaMockKm = 12.5;
    private const int DuracionMockMinutos = 25;

    public MockDistanciaService(IGeocoderService geocoder)
    {
        _geocoder = geocoder;
    }

    public async Task<DistanciaRutaDto> CalcularAsync(string origenDireccion, string destinoDireccion)
    {
        // Geocodifica ambos extremos (también mock por ahora)
        var origen = await _geocoder.GeocodeAsync(origenDireccion);
        var destino = await _geocoder.GeocodeAsync(destinoDireccion);

        // TODO: reemplazar con llamada real a OSRM o Google Directions API
        // usando origen.Lat, origen.Lng, destino.Lat, destino.Lng
        return new DistanciaRutaDto
        {
            Origen = origen,
            Destino = destino,
            DistanciaKm = DistanciaMockKm,
            DuracionMinutos = DuracionMockMinutos
        };
    }
}
