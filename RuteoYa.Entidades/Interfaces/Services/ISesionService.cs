using RuteoYa.Entidades.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Interfaces.Services
{
    public interface ISesionService
    {
        Task<UsuarioDto?> IniciarSesionAsync(IniciarSesionDto iniciarSesion);
        Task<bool> CerrarSesionAsync();
    }
}
