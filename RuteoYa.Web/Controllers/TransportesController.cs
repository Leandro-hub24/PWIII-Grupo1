using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Web.Services;
using System.Security.Claims;

namespace RuteoYa.Web.Controllers;

/// <summary>
/// Pantalla 12 (Mis transportes): el conductor gestiona los paquetes que aceptó llevar.
/// </summary>
[Authorize]
public class TransportesController : Controller
{
    private readonly ITransportesApiService _transportesApi;

    public TransportesController(ITransportesApiService transportesApi)
    {
        _transportesApi = transportesApi;
    }

    // GET: /Transportes?estado=3
    public async Task<IActionResult> Index(EstadoEnvio? estado)
    {
        string? nameUsuario = User.FindFirstValue(ClaimTypes.Name);
        ViewData["nombreUsuario"] = nameUsuario ?? "Invitado";
        ViewData["ActivePage"] = "Transportes";
        ViewBag.EstadoActual = estado;
        var transportes = await _transportesApi.GetMisTransportesAsync(estado);
        return View(transportes);
    }

    // POST: /Transportes/CambiarEstado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoEnvio nuevo, EstadoEnvio? estadoFiltro)
    {
        var error = await _transportesApi.CambiarEstadoAsync(id, nuevo);
        if (error == null) TempData["Mensaje"] = "Estado actualizado.";
        else TempData["Error"] = error;
        return RedirectToAction(nameof(Index), new { estado = estadoFiltro });
    }
}