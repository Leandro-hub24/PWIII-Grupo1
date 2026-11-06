using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Entidades.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Identity.Client.NativeInterop;

namespace RuteoYa.Logica.Services;

public class SesionService : ISesionService
{
    private readonly IUsuariosRepositorio _usuariosRepositorio;
    public SesionService(IUsuariosRepositorio usuariosRepositorio   )
    {
        _usuariosRepositorio = usuariosRepositorio;
    }

    public Task<bool> CerrarSesionAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<UsuarioDto?> IniciarSesionAsync(IniciarSesionDto iniciarSesion)
    {

        var usuario = await _usuariosRepositorio.GetUsuarioPorEmailAsync(iniciarSesion.Email);
        if (usuario == null)
        {
            throw new Exception("Usuario no encontrado.");
        }

        if (!BCrypt.Net.BCrypt.Verify(iniciarSesion.Password, usuario.PasswordHash))
        {
            throw new Exception("Credenciales incorrectas.");
        }

        return new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.PasswordHash, usuario.CreadoEn);
    }

}
