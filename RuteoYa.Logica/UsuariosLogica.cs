using RuteoYa.Entidades.Repositorios;
using RuteoYa.Entidades;
namespace RuteoYa.Logica
{   
    public interface IUsuariosLogica
    {
        Task<List<Usuario>> GetUsuariosAsync();
        Task<Usuario?> GetUsuarioPorIdAsync(int id);
        Task<Usuario?> GetUsuarioPorEmailAsync(string email);
        Task<bool> CreateUsuarioAsync(Usuario usuario);
        Task<bool> UpdateUsuarioAsync(int id, Usuario usuario);
        Task<bool> DeleteUsuarioAsync(int id);
    }
    public class UsuariosLogica : IUsuariosLogica
    {
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public UsuariosLogica(IUsuariosRepositorio usuariosRepositorio)
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

        public async Task<Usuario?> GetUsuarioPorEmailAsync(string email)
        {
            return await _usuariosRepositorio.GetUsuarioPorEmail(email);
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
