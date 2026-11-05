using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Entidades.Interfaces.Services;

public interface IOportunidadesService
{
    Task<Viaje?> ObtenerViajePorIdAsync(int viajeId);
    Task<List<Envio>> ObtenerEnviosCompatiblesAsync(int viajeId, string? orden);
    Task<Envio?> ObtenerEnvioPorIdAsync(int envioId);
    Task<DetalleOportunidadDto?> CalcularDetalleOportunidadAsync(int viajeId, int envioId);
    Task<bool> AceptarOportunidadAsync(int envioId, int viajeId);
}
