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
    }
}
