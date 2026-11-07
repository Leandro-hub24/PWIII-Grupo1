using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Entidades.Interfaces.Repositories;

public interface IEnviosRepositorio
{
    Task<List<Envio>> GetEnviosDeUsuarioAsync(int usuarioId, EstadoEnvio? estado);
    /// <summary>Trae el envío con su historial y su asignación/viaje.</summary>
    Task<Envio?> GetEnvioPorIdAsync(int id);
    Task CrearAsync(Envio envio);
    /// <summary>Cambia el estado y agrega la fila en EnvioEstadoHistorial.</summary>
    Task CambiarEstadoAsync(Envio envio, EstadoEnvio nuevo);
}
