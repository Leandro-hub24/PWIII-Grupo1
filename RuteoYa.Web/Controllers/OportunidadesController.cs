using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Web.Logic;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Controllers;

[Authorize]
public class OportunidadesController : Controller
{
    private readonly OportunidadesOrchestrator _orchestrator;
    private readonly IViajesApiService _viajesApi;
    public OportunidadesController(OportunidadesOrchestrator orchestrator, IViajesApiService viajesApi)
    {
        _orchestrator = orchestrator;
        _viajesApi = viajesApi;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? viajeId = null, string? orden = "ganancia")
    {
        ViewData["ActivePage"] = "Oportunidades";

        int idSeleccionado;

        
        if (viajeId.HasValue && viajeId.Value > 0)
        {
            idSeleccionado = viajeId.Value;
        }
        else
        {
            var misViajes = await _viajesApi.GetMisViajesAsync(activo: true);

            if (!misViajes.Any())
            {
                TempData["Error"] = "Primero debés crear un viaje activo para ver oportunidades.";
                return RedirectToAction("Index", "Viajes");
            }

            idSeleccionado = misViajes.First().Id;
        }

        var envios = await _orchestrator.ObtenerOportunidadesAsync(idSeleccionado, orden);
        ViewBag.ViajeId = idSeleccionado;
        ViewBag.OrdenActual = orden;
        return View(envios);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id, int viajeId)
    {
        ViewData["ActivePage"] = "Oportunidades";
        var detalle = await _orchestrator.ObtenerDetalleOportunidadAsync(id, viajeId);
        if (detalle == null)
        {
            return NotFound("No se pudo cargar el detalle de la oportunidad.");
        }
        return View(detalle);
    }

    [HttpPost]
    public async Task<IActionResult> AceptarEnvio(int envioId, int viajeId)
    {
        var resultado = await _orchestrator.AceptarAsync(envioId, viajeId);
        if (!resultado.Success)
        {
            TempData["Error"] = resultado.Message;
            return RedirectToAction("Detalle", new { id = envioId, viajeId });
        }

        TempData["Exito"] = resultado.Message;
        return RedirectToAction("Index", "Transportes");
    }
}
