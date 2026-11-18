using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

public interface IOptimizadorService
{
    Task<ResultadoOptimizacionDto?> ObtenerOptimizacionPorViajeAsync(int viajeId, int usuarioId);
    Task<ResultadoOperacion<string>> ConfirmarCombinacionAsync(ConfirmarAsignacionDto dto, int usuarioId);
    ResultadoOptimizacionDto CalcularMejorCombinacion(RestriccionesViajeDto restricciones, List<OportunidadDto> candidatos);
    Task<List<ViajeResumenDto>> ObtenerViajesActivosAsync(int usuarioId);
    Task<ResultadoOperacion<PotencialMensualDto>> CalcularPotencialMensualAsync( int usuarioId, int anio, int mes, string criterio = "ganancias");
}