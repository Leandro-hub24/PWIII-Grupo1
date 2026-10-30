using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;




public class UsuariosRepositorio : IUsuariosRepositorio
{
    private readonly RuteoYaDbContext _context;
    public UsuariosRepositorio(RuteoYaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> GetUsuarios()
    {
        return await _context.Usuarios.ToListAsync();
    }               

    public async Task<Usuario?> GetUsuarioPorId(int id)
    {
        return await _context.Usuarios.FindAsync(id).AsTask();
    }

    public async Task<UsuarioDto?> GetUsuarioPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .Where(u => u.Email == email)
            .Select(u => new UsuarioDto(u.Id, u.Nombre, u.Apellido, u.Email, u.PasswordHash, u.CreadoEn))
            .FirstOrDefaultAsync();
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
