using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SesionController : ControllerBase
{
    
    private readonly ISesionService _sesionService;
    private readonly TokenService _tokenService;

    public SesionController(ISesionService sesionService, TokenService tokenService)
    {
        _sesionService = sesionService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> IniciarSesion([FromBody] IniciarSesionDto iniciarSesion)
    {

        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuario = await _sesionService.IniciarSesionAsync(iniciarSesion);
            var token = _tokenService.GenerarTokenJwt(usuario);
            return Ok(new { Token = token, Id = usuario.Id, NombreApellido = usuario.Nombre + " " + usuario.Apellido });
        }
        catch (Exception ex)
        {
            // Si el mensaje es el que lanzamos nosotros de forma controlada, es un Bad Request (400)
            if (ex.Message == "Usuario no encontrado." || ex.Message == "Credenciales incorrectas.")
            {
                return BadRequest(new { Message = ex.Message });
            }

            // Si es un error de base de datos, red, timeout o cualquier otra falla no controlada, devolvemos 500
            return StatusCode(500, new { Message = "Error interno en el servidor. Intente más tarde.", Detalle = ex.Message });
        }

    }

}
