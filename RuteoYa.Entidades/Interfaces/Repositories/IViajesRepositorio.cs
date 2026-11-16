using RuteoYa.Entidades.Entities;

namespace RuteoYa.Entidades.Interfaces.Repositories;

public interface IViajesRepositorio
{
    Task<List<Viaje>> GetViajesDeUsuarioAsync(int usuarioId, bool? activo);
    Task<Viaje?> GetViajePorIdAsync(int id);
    Task<List<Envio>> GetEnviosAsignadosAsync(int viajeId);
    Task CrearAsync(Viaje viaje);
    Task ActualizarAsync(Viaje viaje);
}
