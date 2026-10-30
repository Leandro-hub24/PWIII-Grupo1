using System.Net.Http.Json;
using System;
using RuteoYa.Entidades.Entities;

namespace RuteoYa.Web.Services;

public interface IUsuariosService
{
    Task<Usuario?> GetUsuarioByIdAsync(int id);
}
public class UsuariosService : IUsuariosService
{
    public async Task<Usuario?> GetUsuarioByIdAsync(int id)
    {
        using var client = new HttpClient();
        var result = await client.GetFromJsonAsync<Usuario?>("https://localhost:7019/api/usuarios/{id}");
        return result;
        
    }

}
