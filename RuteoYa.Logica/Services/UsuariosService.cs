using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;
namespace RuteoYa.Logica.Services

{   

    public class UsuariosService : IUsuariosService
    {
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public UsuariosService(IUsuariosRepositorio usuariosRepositorio)
        {
            _usuariosRepositorio = usuariosRepositorio;
        }

        public async Task<bool> CreateUsuarioAsync(Usuario usuario)
        {
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

            usuario.CreadoEn = DateTime.Now;

            return await _usuariosRepositorio.CreateUsuario(usuario);
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            return await _usuariosRepositorio.DeleteUsuario(id);
        }

        public async Task<UsuarioDto?> GetUsuarioPorEmailAsync(string email)
        {
            var usuario = await _usuariosRepositorio.GetUsuarioPorEmailAsync(email);
            if (usuario == null)
            {
                return null;
            }

            return usuario;
        }

        public async Task<Usuario?> GetUsuarioPorIdAsync(int id)
        {
            return await _usuariosRepositorio.GetUsuarioPorId(id);
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await _usuariosRepositorio.GetUsuarios();
        }

        public async Task<bool> UpdateUsuarioAsync(int id, Usuario usuario)
        {
            return await _usuariosRepositorio.UpdateUsuario(id, usuario);
        }
        
    }
}
