namespace RuteoYa.Entidades.DTOs;

/// <summary>
/// Resultado de geocodificar una dirección textual.
/// </summary>
public class GeocoderDto
{
    public string Direccion { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}
