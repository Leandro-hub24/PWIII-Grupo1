using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Logic
{
    public class OportunidadesOrchestrator
    {
        private readonly IOportunidadesApiService _apiService;

        public OportunidadesOrchestrator(IOportunidadesApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<PaginadoOportunidadesDto<Envio>> ObtenerOportunidadesAsync(int viajeId, string? orden, int paguinaActual, int tamanioDePaguina)
        {
            return await _apiService.ObtenerEnviosCompatiblesAsync(viajeId, orden, paguinaActual, tamanioDePaguina);
        }

        public async Task<DetalleOportunidadDto?> ObtenerDetalleOportunidadAsync(int envioId, int viajeId)
        {
            return await _apiService.ObtenerDetalleAsync(envioId, viajeId);
        }

        public async Task<(bool Success, string Message)> AceptarAsync(int envioId, int viajeId)
        {
            return await _apiService.AceptarEnvioAsync(envioId, viajeId);
        }
    }
}
