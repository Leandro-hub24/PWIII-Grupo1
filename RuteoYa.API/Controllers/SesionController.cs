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
            return BadRequest(new { Message = ex.Message });
        }

    }

}
