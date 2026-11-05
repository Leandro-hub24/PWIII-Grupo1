using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Web.Services;

public interface IOportunidadesApiService
{
    Task<List<Envio>> ObtenerEnviosCompatiblesAsync(int viajeId, string? orden);
    Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int envioId, int viajeId);
    Task<(bool Success, string Message)> AceptarEnvioAsync(int envioId, int viajeId);
}
