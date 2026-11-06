using Microsoft.Extensions.Configuration;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RuteoYa.Logica.Services;

/// <summary>
/// Implementación de cálculo de distancia usando HERE Routing API.
/// Geocodifica ambas direcciones y calcula la ruta óptima en auto.
/// </summary>
public class DistanciaService : IDistanciaService
{
    private readonly IGeocoderService _geocoder;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public DistanciaService(IGeocoderService geocoder, HttpClient httpClient, IConfiguration configuration)
    {
        _geocoder = geocoder;
        _httpClient = httpClient;
        _apiKey = configuration["HereApi:ApiKey"] ?? throw new InvalidOperationException("HERE API Key not configured.");
    }

    public async Task<DistanciaRutaDto> CalcularAsync(string origenDireccion, string destinoDireccion)
    {
        // Geocodifica ambos extremos
        var origen = await _geocoder.ObtenerCoordenadasAsync(origenDireccion);
        var destino = await _geocoder.ObtenerCoordenadasAsync(destinoDireccion);

        // Usar InvariantCulture para forzar punto como separador decimal
        var origenCoords = $"{origen.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{origen.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        var destinoCoords = $"{destino.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{destino.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        var url = $"https://router.hereapi.com/v8/routes" +
                  $"?transportMode=car" +
                  $"&origin={origenCoords}" +
                  $"&destination={destinoCoords}" +
                  $"&return=summary" +
                  $"&apiKey={_apiKey}";

        var response = await _httpClient.GetAsync(url);

        // Leer el contenido de la respuesta
        var jsonContent = await response.Content.ReadAsStringAsync();

        // Si hay error, mostrar el contenido para debugging
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Error en HERE Routing API (Status: {response.StatusCode}). " +
                $"Respuesta: {jsonContent}");
        }

        // Deserializar la respuesta
        var resultado = System.Text.Json.JsonSerializer.Deserialize<HereRoutingResponse>(
            jsonContent, 
            new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

        if (resultado?.Routes == null || resultado.Routes.Count == 0)
        {
            throw new InvalidOperationException("No se encontró ninguna ruta entre los puntos especificados.");
        }

        var rutaPrincipal = resultado.Routes[0];

        if (rutaPrincipal?.Sections == null || rutaPrincipal.Sections.Count == 0)
        {
            throw new InvalidOperationException("La ruta no contiene secciones.");
        }

        var primeraSeccion = rutaPrincipal.Sections[0];

        if (primeraSeccion?.Summary == null)
        {
            throw new InvalidOperationException("La sección no contiene información de resumen.");
        }

        return new DistanciaRutaDto
        {
            Origen = origen,
            Destino = destino,
            DistanciaKm = primeraSeccion.Summary.Length / 1000.0, // metros → km
            DuracionMinutos = (int)Math.Round(primeraSeccion.Summary.Duration / 60.0) // segundos → minutos
        };
    }

    private class HereRoutingResponse
    {
        [JsonPropertyName("routes")]
        public List<HereRouteItem> Routes { get; set; } = new();
    }

    private class HereRouteItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("sections")]
        public List<HereRouteSection> Sections { get; set; } = new();
    }

    private class HereRouteSection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public HereRouteSummary Summary { get; set; } = new();
    }

    private class HereRouteSummary
    {
        /// <summary>Distancia en METROS (HERE usa "length" en vez de "distance")</summary>
        [JsonPropertyName("length")]
        public int Length { get; set; }

        /// <summary>Duración en SEGUNDOS</summary>
        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("baseDuration")]
        public int BaseDuration { get; set; }
    }
}
