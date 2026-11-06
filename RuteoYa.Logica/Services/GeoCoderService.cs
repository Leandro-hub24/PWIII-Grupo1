using Microsoft.Extensions.Configuration;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RuteoYa.Logica.Services;

/// <summary>
/// Implementación mock de geocodificación.
/// Devuelve coordenadas fijas hasta integrar una API real (Nominatim, Google Maps, etc.).
/// </summary>
public class GeocoderService : IGeocoderService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeocoderService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["HereApi:ApiKey"] ?? throw new InvalidOperationException("HERE API Key not configured.");
    }


    public async Task<GeocoderDto> ObtenerCoordenadasAsync(string direccion)
    {
        // Codificar la dirección por si tiene espacios o caracteres especiales (ej: "5 Rue Daunou, Paris")
        var direccionCodificada = Uri.EscapeDataString(direccion);

        var url = $"https://geocode.search.hereapi.com/v1/geocode?q={direccionCodificada}&apiKey={_apiKey}";

        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var resultado = await response.Content.ReadFromJsonAsync<HereGeocodingResponse>();

        var primerItem = resultado.Items[0];

        var result = new GeocoderDto
        {
            Direccion = primerItem.Address?.Label ?? direccion,
            Lat = (decimal)primerItem.Position.Lat,
            Lng = (decimal)primerItem.Position.Lng
        };



        return (result);
    }

    // ========================================================================
    // DTOs INTERNOS para deserializar la respuesta de HERE
    // ========================================================================

    /// <summary>
    /// Respuesta raíz de HERE Geocoding API
    /// </summary>
    private class HereGeocodingResponse
    {
        [JsonPropertyName("items")]
        public List<HereGeocodingItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Item individual en el array de resultados
    /// </summary>
    private class HereGeocodingItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public HereAddress? Address { get; set; }

        [JsonPropertyName("position")]
        public HerePosition Position { get; set; } = new();
    }

    /// <summary>
    /// Dirección estructurada
    /// </summary>
    private class HereAddress
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Coordenadas geográficas
    /// </summary>
    private class HerePosition
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }


}
