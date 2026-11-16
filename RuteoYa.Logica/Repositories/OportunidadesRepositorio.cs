using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Interfaces.Repositories;
namespace RuteoYa.Logica.Repositories;

public class OportunidadesRepositorio : IOportunidadesRepositorio
{
    private readonly RuteoYaDbContext _context;

    public OportunidadesRepositorio(RuteoYaDbContext context)
    { 
        _context = context;
    }

    public Task<Viaje?> GetViajePorIdAsync(int id)
    {
        return _context.Viajes.FirstOrDefaultAsync(v => v.Id == id && v.Activo);
    }

    public Task<List<Envio>> GetEnviosCompatiblesAsync(decimal pesoMaximo)
    {
        return _context.Envios.Where(e => e.Estado == 1 && e.PesoKg <= pesoMaximo).ToListAsync();
    }

    public Task<Envio?> GetEnvioPorIdAsync(int id)
    {
        return _context.Envios.FindAsync(id).AsTask();
    }

    public async Task<(int Paquetes, decimal PesoKg)> GetCargaAsignadaAsync(int viajeId)
    {
        var pesos = await _context.Envios
            .AsNoTracking()
            .Where(e => e.Estado == (byte)EstadoEnvio.ConductorAsignado &&
                        _context.Set<Asignacion>().Any(a => a.Id == e.AsignacionId && a.ViajeId == viajeId))
            .Select(e => e.PesoKg)
            .ToListAsync();

        return (pesos.Count, pesos.Sum());
    }

    public async Task<bool> AsignarEnvioAsync(int envioId, int viajeId, byte nuevoEstado)
    {
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        
        var envio = await _context.Envios.FindAsync(envioId);
        if (envio == null || envio.Estado != (byte)EstadoEnvio.BuscandoConductor)
        {
            await transaccion.RollbackAsync();
            return false;
        }

        var asignacion = new Asignacion
        {
            ViajeId = viajeId,
            FechaViaje = DateTime.Now,
            Ganancia = envio.Monto,
            AsignadoEn = DateTime.Now
        };
        _context.Set<Asignacion>().Add(asignacion);
        await _context.SaveChangesAsync();

        
        var filas = await _context.Envios
            .Where(e => e.Id == envioId && e.Estado == (byte)EstadoEnvio.BuscandoConductor)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Estado, nuevoEstado)
                .SetProperty(e => e.AsignacionId, asignacion.Id));

        if (filas == 0)
        {
            await transaccion.RollbackAsync();
            return false;
        }

        
        _context.Set<EnvioEstadoHistorial>().Add(new EnvioEstadoHistorial
        {
            EnvioId = envioId,
            EstadoAnterior = (byte)EstadoEnvio.BuscandoConductor,
            EstadoNuevo = nuevoEstado,
            Fecha = DateTime.Now
        });
        await _context.SaveChangesAsync();

        await transaccion.CommitAsync();
        return true;
    }
}
