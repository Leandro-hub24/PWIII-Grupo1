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
        var origen = await _geocoder.ObtenerCoordenadasAsync(origenDireccion);
        var destino = await _geocoder.ObtenerCoordenadasAsync(destinoDireccion);

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

    public async Task<DistanciaRutaDto> CalcularPorCoordenadasAsync(double origenLat, double origenLng, double destinoLat, double destinoLng)
    {
        double diffLat = destinoLat - origenLat;
        double diffLng = destinoLng - origenLng;
        double distanciaEstimadaKm = Math.Sqrt(diffLat * diffLat + diffLng * diffLng) * 111.0;

        if (distanciaEstimadaKm < 0.5) distanciaEstimadaKm = 3.5;

        return await Task.FromResult(new DistanciaRutaDto
        {
            DistanciaKm = Math.Round(distanciaEstimadaKm, 2),
            DuracionMinutos = (int)Math.Round(distanciaEstimadaKm * 2)
        });
    }
}
