using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Entidades.Interfaces.Repositories
{
    public interface IOportunidadesRepositorio
    {
        Task<Viaje?> GetViajePorIdAsync(int id);
        Task<List<Envio>> GetEnviosCompatiblesAsync(decimal pesoMaximo);
        Task<Envio?> GetEnvioPorIdAsync(int id);
        Task<(int Paquetes, decimal PesoKg)> GetCargaAsignadaAsync(int viajeId);
        Task<bool> AsignarEnvioAsync(int envioId, int viajeId, byte nuevoEstado);

        // Metodos Agregados
        Task<(bool Exito, string? Mensaje)> AsignarEnviosViajesAsync(
    int viajeId, List<AsignacionEnvioDetalleDto> envios, DateTime fechaViaje);
        Task<List<Envio>> GetEnviosAsignadosAViajeAsync(int viajeId);
        Task<List<Viaje>> GetViajesActivosDeUsuarioAsync(int usuarioId);
        Task<List<Envio>> GetEnviosPorIdsAsync(List<int> ids);
        Task<decimal> GetGananciaEfectivaMesAsync(int conductorId, int anio, int mes);
    }
}
