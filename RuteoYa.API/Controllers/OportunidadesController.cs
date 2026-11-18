using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Services;
using System.Security.Claims;

namespace RuteoYa.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OportunidadesController : ControllerBase
{
    private readonly IOportunidadesService _oportunidadesService;

    public OportunidadesController(IOportunidadesService oportunidadesService)
    {
        _oportunidadesService = oportunidadesService;
    }

    [HttpGet("compatibles")]
    public async Task<ActionResult<List<Envio>>> GetEnviosCompatibles([FromQuery] int viajeId, [FromQuery] string? orden)
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        var viaje = await _oportunidadesService.ObtenerViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return NotFound(new { mensaje = "Viaje no encontrado o no te pertenece." });

        var envios = await _oportunidadesService.ObtenerEnviosCompatiblesAsync(viajeId, orden);
        return Ok(envios);
    }

    [HttpGet("detalle/{id:int}")]
    public async Task<ActionResult> GetDetalleOportunidad(int id, [FromQuery] int viajeId)
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        var viaje = await _oportunidadesService.ObtenerViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return NotFound(new { mensaje = "Viaje no encontrado o no te pertenece." });

        var detalle = await _oportunidadesService.CalcularDetalleOportunidadAsync(viajeId, id);
        if (detalle == null)
        {
            return NotFound("No se encontro el envio o el viaje.");
        }
        return Ok(detalle);
    }

    [HttpPost("aceptar")]
    public async Task<ActionResult> AceptarOportunidad([FromBody] AceptarOportunidadDto dto)
    {
        if (UsuarioId() is not int usuarioId)
            return Unauthorized();

        var viaje = await _oportunidadesService.ObtenerViajePorIdAsync(dto.ViajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return NotFound(new { mensaje = "Viaje no encontrado o no te pertenece." });

        try
        {
            var resultado = await _oportunidadesService.AceptarOportunidadAsync(dto.EnvioId, dto.ViajeId);

            if (!resultado.Exito)
            {
                return BadRequest(new { Message = resultado.Mensaje });
            }

            return Ok(new { Message = resultado.Mensaje });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { Message = ex.Message });
        }
    }

    private int? UsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out var id) ? id : null;
    }

} 
