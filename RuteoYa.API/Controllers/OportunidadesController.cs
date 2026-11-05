using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

[Route("api/[controller]")]
[ApiController]
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
        var envios = await _oportunidadesService.ObtenerEnviosCompatiblesAsync(viajeId, orden);
        return Ok(envios);
    }

    [HttpGet("detalle/{id:int}")]
    public async Task<ActionResult> GetDetalleOportunidad(int id, [FromQuery] int viajeId)
    { 
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
        var exito = await _oportunidadesService.AceptarOportunidadAsync(dto.EnvioId, dto.ViajeId);
        if (!exito)
        {
            return BadRequest(new { Message = "No se pudo aceptar el envío, es posible que ya no esté disponible." });
        }
        return Ok(new { Message = "Envío aceptado exitosamente" });
    }

} 
