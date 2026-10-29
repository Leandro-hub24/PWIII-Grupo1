using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

/// <summary>
/// Interfaz para el servicio que consume la API de viajes.
/// </summary>
public interface IViajesApiService
{
    Task<DistanciaRutaDto?> CalcularDistanciaAsync(string origenDireccion, string destinoDireccion);
}
