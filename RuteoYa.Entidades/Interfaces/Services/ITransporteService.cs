using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;

namespace RuteoYa.Entidades.Interfaces.Services;

public interface ITransportesService
{
    Task<List<TransporteDto>> GetMisTransportesAsync(int conductorId, EstadoEnvio? estado);
    Task<ResultadoOperacion<TransporteDto>> CambiarEstadoAsync(int conductorId, int envioId, EstadoEnvio nuevo);
}