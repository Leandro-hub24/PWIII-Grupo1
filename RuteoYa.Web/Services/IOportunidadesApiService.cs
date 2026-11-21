using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Web.Services;

public interface IOportunidadesApiService
{
    Task<PaginadoOportunidadesDto<Envio>> ObtenerEnviosCompatiblesAsync(int viajeId, string? orden, int paginaActual, int tamanioDePagina);
    Task<DetalleOportunidadDto?> ObtenerDetalleAsync(int envioId, int viajeId);
    Task<(bool Success, string Message)> AceptarEnvioAsync(int envioId, int viajeId);
}
