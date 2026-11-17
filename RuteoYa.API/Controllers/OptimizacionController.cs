using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

/// <summary>
/// Optimización y reserva de combinaciones de envíos para un viaje (Pantallas 10 y 11).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OptimizacionController : ControllerBase
{
    private readonly IOptimizadorService _optimizadorService;

    public OptimizacionController(IOptimizadorService optimizadorService)
    {
        _optimizadorService = optimizadorService;
    }

    /// <summary>Obtiene los viajes activos del usuario autenticado para seleccionar en el optimizador.</summary>
    [HttpGet("mis-viajes")]
    [ProducesResponseType(typeof(List<ViajeResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ViajeResumenDto>>> GetMisViajesActivos()
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        return Ok(await _optimizadorService.ObtenerViajesActivosAsync(usuarioId));
    }

    /// <summary>Calcula la mejor combinación de envíos compatibles para un viaje.</summary>
    [HttpGet("{viajeId:int}")]
    [ProducesResponseType(typeof(ResultadoOptimizacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultadoOptimizacionDto>> ObtenerOptimizacion(int viajeId)
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        var resultado = await _optimizadorService.ObtenerOptimizacionPorViajeAsync(viajeId, usuarioId);
        if (resultado == null)
            return NotFound(new { mensaje = "Viaje no encontrado o inactivo." });

        return Ok(resultado);
    }

    /// <summary>Confirma y reserva en bloque los envíos seleccionados para el viaje.</summary>
    [HttpPost("confirmar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarAsignacionDto dto)
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        if (dto == null || dto.EnvioIds == null || !dto.EnvioIds.Any())
            return BadRequest(new { mensaje = "Debe enviar al menos un paquete para reservar." });

        var resultado = await _optimizadorService.ConfirmarCombinacionAsync(dto, usuarioId);
        if (resultado.Tipo == TipoResultado.Ok)
            return StatusCode(StatusCodes.Status201Created, new { mensaje = resultado.Valor });

        return Responder(resultado);
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

    /// <summary>Estima el potencial de ganancias mensual analizando viajes recurrentes y envíos disponibles (Pantalla 14).</summary>
    [HttpGet("potencial-mensual")]
    [ProducesResponseType(typeof(PotencialMensualDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PotencialMensualDto>> GetPotencialMensual(
        [FromQuery] int? anio,
        [FromQuery] int? mes,
        [FromQuery] string criterio = "ganancias")
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        int anioConsulta = anio ?? DateTime.Today.Year;
        int mesConsulta = mes ?? DateTime.Today.Month;

        return Responder(await _optimizadorService.CalcularPotencialMensualAsync(
            usuarioId, anioConsulta, mesConsulta, criterio));
    }
}