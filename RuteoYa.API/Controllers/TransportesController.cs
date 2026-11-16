using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

/// <summary>
/// Paquetes que el usuario autenticado aceptó llevar como conductor (Pantalla 12 - Mis transportes).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class TransportesController : ControllerBase
{
    private readonly ITransportesService _transportesService;

    public TransportesController(ITransportesService transportesService)
    {
        _transportesService = transportesService;
    }

    /// <summary>Lista los transportes propios. Filtro opcional por estado.</summary>
    /// <response code="200">Lista de transportes.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransporteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TransporteDto>>> GetMisTransportes([FromQuery] EstadoEnvio? estado)
    {
        if (UsuarioId() is not int conductorId) return Unauthorized();
        return Ok(await _transportesService.GetMisTransportesAsync(conductorId, estado));
    }

    /// <summary>Marca el paquete como retirado.</summary>
    /// <response code="200">Estado actualizado.</response>
    /// <response code="404">No existe o no es un transporte del usuario.</response>
    /// <response code="409">La transición de estado no es válida.</response>
    [HttpPut("{envioId:int}/retirar")]
    [ProducesResponseType(typeof(TransporteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<ActionResult<TransporteDto>> Retirar(int envioId)
        => CambiarEstado(envioId, EstadoEnvio.Retirado);

    /// <summary>Marca el paquete como en camino.</summary>
    /// <response code="200">Estado actualizado.</response>
    /// <response code="404">No existe o no es un transporte del usuario.</response>
    /// <response code="409">La transición de estado no es válida.</response>
    [HttpPut("{envioId:int}/en-camino")]
    [ProducesResponseType(typeof(TransporteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<ActionResult<TransporteDto>> EnCamino(int envioId)
        => CambiarEstado(envioId, EstadoEnvio.EnCamino);

    /// <summary>Marca el paquete como entregado (solo si estaba en camino).</summary>
    /// <response code="200">Estado actualizado.</response>
    /// <response code="404">No existe o no es un transporte del usuario.</response>
    /// <response code="409">La transición de estado no es válida.</response>
    [HttpPut("{envioId:int}/entregar")]
    [ProducesResponseType(typeof(TransporteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<ActionResult<TransporteDto>> Entregar(int envioId)
        => CambiarEstado(envioId, EstadoEnvio.Entregado);

    private async Task<ActionResult<TransporteDto>> CambiarEstado(int envioId, EstadoEnvio nuevo)
    {
        if (UsuarioId() is not int conductorId) return Unauthorized();
        return Responder(await _transportesService.CambiarEstadoAsync(conductorId, envioId, nuevo));
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