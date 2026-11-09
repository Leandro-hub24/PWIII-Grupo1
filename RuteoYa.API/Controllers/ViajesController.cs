using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class ViajesController : ControllerBase
{
    private readonly IViajesService _viajesService;

    public ViajesController(IViajesService viajesService)
    {
        _viajesService = viajesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ViajeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ViajeDto>>> GetMisViajes([FromQuery] bool? activo)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Ok(await _viajesService.GetMisViajesAsync(usuarioId, activo));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ViajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ViajeDto>> GetViaje(int id)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Responder(await _viajesService.GetViajeAsync(usuarioId, id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ViajeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ViajeDto>> CrearViaje([FromBody] CrearViajeDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (UsuarioId() is not int usuarioId) return Unauthorized();

        var resultado = await _viajesService.CrearViajeAsync(usuarioId, dto);
        if (resultado.Tipo != TipoResultado.Ok) return Responder(resultado);

        return CreatedAtAction(nameof(GetViaje), new { id = resultado.Valor!.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}/desactivar")]
    [ProducesResponseType(typeof(ViajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ViajeDto>> DesactivarViaje(int id)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Responder(await _viajesService.DesactivarViajeAsync(usuarioId, id));
    }

    [HttpPut("{id:int}/activar")]
    [ProducesResponseType(typeof(ViajeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ViajeDto>> ActivarViaje(int id)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Responder(await _viajesService.ActivarViajeAsync(usuarioId, id));
    }

    private int? UsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out var id) ? id : null;
    }

    private ActionResult Responder<T>(ResultadoOperacion<T> r) => r.Tipo switch
    {
        TipoResultado.Ok => Ok(r.Valor),
        TipoResultado.NoEncontrado => NotFound(new { mensaje = r.Mensaje }),
        TipoResultado.Conflicto => Conflict(new { mensaje = r.Mensaje }),
        _ => BadRequest(new { mensaje = r.Mensaje })
    };
}
