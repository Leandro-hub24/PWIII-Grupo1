using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

/// <summary>
/// Envíos del usuario autenticado (Pantallas 6 y 7).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class EnviosController : ControllerBase
{
    private readonly IEnviosService _enviosService;

    public EnviosController(IEnviosService enviosService)
    {
        _enviosService = enviosService;
    }

    /// <summary>Lista los envíos propios. Filtro opcional por estado.</summary>
    /// <response code="200">Lista de envíos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EnvioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EnvioDto>>> GetMisEnvios([FromQuery] EstadoEnvio? estado)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Ok(await _enviosService.GetMisEnviosAsync(usuarioId, estado));
    }

    /// <summary>Detalle de un envío propio, con su historial de estados.</summary>
    /// <response code="200">Envío encontrado.</response>
    /// <response code="404">No existe o es de otro usuario.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnvioDto>> GetEnvio(int id)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Responder(await _enviosService.GetEnvioAsync(usuarioId, id));
    }

    /// <summary>Publica un envío nuevo (queda "Buscando conductor").</summary>
    /// <response code="201">Envío creado.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EnvioDto>> CrearEnvio([FromBody] CrearEnvioDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (UsuarioId() is not int usuarioId) return Unauthorized();

        var resultado = await _enviosService.CrearEnvioAsync(usuarioId, dto);
        if (resultado.Tipo != TipoResultado.Ok) return Responder(resultado);

        return CreatedAtAction(nameof(GetEnvio), new { id = resultado.Valor!.Id }, resultado.Valor);
    }

    /// <summary>Cancela un envío propio (solo antes de que lo retiren).</summary>
    /// <response code="200">Envío cancelado.</response>
    /// <response code="404">No existe o es de otro usuario.</response>
    /// <response code="409">El estado actual no permite cancelar.</response>
    [HttpPut("{id:int}/cancelar")]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnvioDto>> CancelarEnvio(int id)
    {
        if (UsuarioId() is not int usuarioId) return Unauthorized();
        return Responder(await _enviosService.CancelarEnvioAsync(usuarioId, id));
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
