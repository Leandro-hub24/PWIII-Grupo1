using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;

namespace RuteoYa.Logica.Repositories;

public class EnviosRepositorio : IEnviosRepositorio
{
    private readonly RuteoYaDbContext _context;

    public EnviosRepositorio(RuteoYaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Envio>> GetEnviosDeUsuarioAsync(int usuarioId, EstadoEnvio? estado)
    {
        var query = _context.Envios
            .Include(e => e.Asignacion)
                .ThenInclude(a => a!.Viaje)
            .Where(e => e.UsuarioId == usuarioId);

        if (estado.HasValue)
        {
            var estadoByte = (byte)estado.Value;
            query = query.Where(e => e.Estado == estadoByte);
        }

        return await query.OrderByDescending(e => e.CreadoEn).ToListAsync();
    }

    public async Task<Envio?> GetEnvioPorIdAsync(int id)
    {
        return await _context.Envios
            .Include(e => e.EnvioEstadoHistorial)
            .Include(e => e.Asignacion)
                .ThenInclude(a => a!.Viaje)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CrearAsync(Envio envio)
    {
        _context.Envios.Add(envio);
        await _context.SaveChangesAsync();
    }

    public async Task CambiarEstadoAsync(Envio envio, EstadoEnvio nuevo)
    {
        _context.EnvioEstadoHistoriales.Add(new EnvioEstadoHistorial
        {
            EnvioId = envio.Id,
            EstadoAnterior = envio.Estado,
            EstadoNuevo = (byte)nuevo,
            Fecha = DateTime.Now
        });
        envio.Estado = (byte)nuevo;
        await _context.SaveChangesAsync();
    }
}
