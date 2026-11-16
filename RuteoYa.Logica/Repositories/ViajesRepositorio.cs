using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;

namespace RuteoYa.Logica.Repositories;

public class ViajesRepositorio : IViajesRepositorio
{
    private readonly RuteoYaDbContext _context;

    public ViajesRepositorio(RuteoYaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Viaje>> GetViajesDeUsuarioAsync(int usuarioId, bool? activo)
    {
        var query = _context.Viajes
            .Include(v => v.ViajeDias)
            .Include(v => v.Paradas.OrderBy(p => p.Orden))
            .Where(v => v.UsuarioId == usuarioId);

        if (activo.HasValue)
            query = query.Where(v => v.Activo == activo.Value);

        return await query.OrderByDescending(v => v.Id).ToListAsync();
    }

    public async Task<Viaje?> GetViajePorIdAsync(int id)
    {
        return await _context.Viajes
            .Include(v => v.ViajeDias)
            .Include(v => v.Paradas.OrderBy(p => p.Orden))
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Envio>> GetEnviosAsignadosAsync(int viajeId)
    {
        return await _context.Envios
            .Include(e => e.Asignacion)
            .Where(e => e.Asignacion != null && e.Asignacion.ViajeId == viajeId)
            .OrderBy(e => e.Asignacion!.FechaViaje)
            .ToListAsync();
    }

    public async Task CrearAsync(Viaje viaje)
    {
        _context.Viajes.Add(viaje);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Viaje viaje)
    {
        await _context.SaveChangesAsync();
    }
}
