using RuteoYa.Entidades.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Interfaces.Repositories
{
    public interface IUsuariosRepositorio
    {
        Task<List<Usuario>> GetUsuarios();
        Task<Usuario?> GetUsuarioPorId(int id);
        Task<Usuario?> GetUsuarioPorEmail(string email);
        Task<bool> CreateUsuario(Usuario usuario);
        Task<bool> UpdateUsuario(int id, Usuario usuario);
        Task<bool> DeleteUsuario(int id);


    }
}
