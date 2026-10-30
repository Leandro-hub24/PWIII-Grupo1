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

    public Task<UsuarioDto?> IniciarSesionAsync(IniciarSesionDto iniciarSesion)
    {

        var usuario = _usuariosRepositorio.GetUsuarioPorEmailAsync(iniciarSesion.Email).Result;
        if (usuario == null)
        {
            throw new Exception("Usuario no encontrado.");
        }

        if (!BCrypt.Net.BCrypt.Verify(iniciarSesion.Password, usuario.PasswordHash))
        {
            throw new Exception("Contraseña incorrecta.");
        }

        return Task.FromResult<UsuarioDto?>(new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.PasswordHash, usuario.CreadoEn));
    }

}
