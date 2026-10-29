using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Interfaces.Repositories;

namespace RuteoYa.Logica.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly RuteoYaDbContext _context;

    public UsuarioRepository(RuteoYaDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .OrderBy(u => u.Apellido)
            .ThenBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var usuario = await ObtenerPorIdAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteEmailAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
