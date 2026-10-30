using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Interfaces.Services
{
    public interface IUsuariosService
    {
        Task<List<Usuario>> GetUsuariosAsync();
        Task<Usuario?> GetUsuarioPorIdAsync(int id);
        Task<UsuarioDto?> GetUsuarioPorEmailAsync(string email);
        Task<bool> CreateUsuarioAsync(Usuario usuario);
        Task<bool> UpdateUsuarioAsync(int id, Usuario usuario);
        Task<bool> DeleteUsuarioAsync(int id);
    }
}
