using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

public interface IEnviosService
{
    Task<List<EnvioDto>> GetMisEnviosAsync(int usuarioId, EstadoEnvio? estado);
    Task<ResultadoOperacion<EnvioDto>> GetEnvioAsync(int usuarioId, int envioId);
    Task<ResultadoOperacion<EnvioDto>> CrearEnvioAsync(int usuarioId, CrearEnvioDto dto);
    Task<ResultadoOperacion<EnvioDto>> CancelarEnvioAsync(int usuarioId, int envioId);
    /// <summary>Para Mis transportes: valida la transición y guarda el historial.</summary>
    Task<ResultadoOperacion<EnvioDto>> CambiarEstadoAsync(int envioId, EstadoEnvio nuevo);
}
