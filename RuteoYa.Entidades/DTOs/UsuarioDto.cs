using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RuteoYa.Entidades.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreadoEn { get; set; }

    public UsuarioDto(int id, string nombre, string apellido, string email, string passwordHash, DateTime creadoEn)
    {
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        Email = email;
        PasswordHash = passwordHash;
        CreadoEn = creadoEn;
    }
}
