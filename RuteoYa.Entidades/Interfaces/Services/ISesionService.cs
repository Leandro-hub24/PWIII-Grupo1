using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Interfaces.Services
{
    public interface ISesionService
    {
        Task<bool> IniciarSesion(string email, string password);
    }
}
