using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

public interface IViajesService
{
    Task<List<ViajeDto>> GetMisViajesAsync(int usuarioId, bool? activo);
    Task<ResultadoOperacion<ViajeDto>> GetViajeAsync(int usuarioId, int viajeId);
    Task<ResultadoOperacion<ViajeDto>> CrearViajeAsync(int usuarioId, CrearViajeDto dto);
    Task<ResultadoOperacion<ViajeDto>> ActualizarViajeAsync(int usuarioId, int viajeId, CrearViajeDto dto);
    Task<ResultadoOperacion<ViajeDto>> DesactivarViajeAsync(int usuarioId, int viajeId);
    Task<ResultadoOperacion<ViajeDto>> ActivarViajeAsync(int usuarioId, int viajeId);
    Task<ResultadoOperacion<RutaViajeDto>> CalcularRutaAsync(CalcularRutaDto dto);
}
