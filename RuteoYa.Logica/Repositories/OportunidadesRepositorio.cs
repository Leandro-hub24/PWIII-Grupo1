using RuteoYa.Entidades;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Entities;
using Microsoft.EntityFrameworkCore;
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

    public async Task<bool> ActualizarEstadoEnvioAsync(int envioId, byte nuevoEstado)
    {
        var envio = await _context.Envios.FindAsync(envioId);
        if (envio == null || envio.Estado != 1) return false;

        envio.Estado = nuevoEstado;
        _context.Envios.Update(envio);
        return await _context.SaveChangesAsync() > 0;
    }
}
