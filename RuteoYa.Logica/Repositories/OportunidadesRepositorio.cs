using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
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
        //return _context.Viajes.FirstOrDefaultAsync(v => v.Id == id && v.Activo);
        return _context.Viajes
            .Include(v => v.ViajeDias)
            .Include(v => v.Paradas.OrderBy(p => p.Orden))
            .FirstOrDefaultAsync(v => v.Id == id && v.Activo);
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


    // Metodos Agregados

    public async Task<(bool Exito, string? Mensaje)> AsignarEnviosViajesAsync(
    int viajeId, List<AsignacionEnvioDetalleDto> envios, DateTime fechaViaje)
    {
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        try
        {
            const byte estadoBuscando = (byte)EstadoEnvio.BuscandoConductor;
            const byte estadoAsignado = (byte)EstadoEnvio.ConductorAsignado;

            foreach (var item in envios)
            {
                var asignacion = new Asignacion
                {
                    ViajeId = viajeId,
                    FechaViaje = fechaViaje,
                    DesvioKm = item.DesvioKm,
                    TiempoExtraMin = item.TiempoExtraMin,
                    Ganancia = item.Ganancia,
                    AsignadoEn = DateTime.Now
                };

                _context.Set<Asignacion>().Add(asignacion);
                await _context.SaveChangesAsync();

                var filas = await _context.Envios
                    .Where(e => e.Id == item.EnvioId && e.Estado == estadoBuscando)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.Estado, estadoAsignado)
                        .SetProperty(e => e.AsignacionId, asignacion.Id));

                if (filas == 0)
                {
                    await transaccion.RollbackAsync();
                    return (false, $"El envío #{item.EnvioId} ya no está disponible.");
                }

                _context.Set<EnvioEstadoHistorial>().Add(new EnvioEstadoHistorial
                {
                    EnvioId = item.EnvioId,
                    EstadoAnterior = estadoBuscando,
                    EstadoNuevo = estadoAsignado,
                    Fecha = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
            return (true, null);
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    public Task<List<Envio>> GetEnviosPorIdsAsync(List<int> ids)
    {
        return _context.Envios.Where(e => ids.Contains(e.Id)).ToListAsync();
    }

    public Task<List<Envio>> GetEnviosAsignadosAViajeAsync(int viajeId)
    {
        return _context.Envios
            .Include(e => e.Asignacion)
            .Where(e => e.Asignacion != null
                     && e.Asignacion.ViajeId == viajeId
                     && e.Estado >= (byte)EstadoEnvio.ConductorAsignado && e.Estado <= (byte)EstadoEnvio.EnCamino)
            .ToListAsync();
    }

    public Task<List<Viaje>> GetViajesActivosDeUsuarioAsync(int usuarioId)
    {
        return _context.Viajes
            .Include(v => v.ViajeDias)
            .Include(v => v.Paradas.OrderBy(p => p.Orden))
            .Where(v => v.UsuarioId == usuarioId && v.Activo)
            .OrderByDescending(v => v.Id)
            .ToListAsync();
    }

    public async Task<decimal> GetGananciaEfectivaMesAsync(int conductorId, int anio, int mes)
    {
        return await _context.Envios
            .Where(e => e.Asignacion != null
                     && e.Asignacion.Viaje != null
                     && e.Asignacion.Viaje.UsuarioId == conductorId
                     && e.Estado == (byte)EstadoEnvio.Entregado
                     && e.Asignacion.FechaViaje.Year == anio
                     && e.Asignacion.FechaViaje.Month == mes)
            .SumAsync(e => e.Asignacion!.Ganancia);
    }



}
