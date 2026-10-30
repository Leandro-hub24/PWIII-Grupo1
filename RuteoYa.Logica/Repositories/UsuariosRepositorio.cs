using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Logica.Repositories;



public class UsuariosRepositorio : IUsuariosRepositorio
{
    private readonly RuteoYaDbContext _context;
    public UsuariosRepositorio(RuteoYaDbContext context)
    {
        _context = context;
    }

    public Task<List<Usuario>> GetUsuarios()
    {
        return _context.Usuarios.ToListAsync();
    }               

    public Task<Usuario?> GetUsuarioPorId(int id)
    {
        return _context.Usuarios.FindAsync(id).AsTask();
    }

    public Task<Usuario?> GetUsuarioPorEmail(string email)
    {
        return _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> CreateUsuario(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUsuario(int id, Usuario usuario)
    {
        var existing = await _context.Usuarios.FindAsync(id);
        if (existing == null) return false;
        _context.Entry(existing).CurrentValues.SetValues(usuario);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return false;
        _context.Usuarios.Remove(usuario);
        return await _context.SaveChangesAsync() > 0;
    }

    
}
