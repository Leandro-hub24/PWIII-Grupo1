using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Web.Services;

public interface IOptimizacionApiService
{
    Task<(ResultadoOptimizacionDto? Data, string? Error)> ObtenerOptimizacionAsync(int viajeId);
    Task<(bool Success, string Message)> ConfirmarCombinacionAsync(int viajeId, List<int> envioIds);
    Task<List<ViajeResumenDto>> ObtenerViajesActivosAsync();
}