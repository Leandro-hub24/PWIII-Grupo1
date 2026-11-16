using Microsoft.AspNetCore.Mvc;
using RuteoYa.Web.Logic;

namespace RuteoYa.Web.Controllers;

public class OportunidadesController : Controller
{
    private readonly OportunidadesOrchestrator _orchestrator;
    public OportunidadesController(OportunidadesOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int viajeId = 1, string? orden = "ganancia")
    {
        var envios = await _orchestrator.ObtenerOportunidadesAsync(viajeId, orden);
        ViewBag.ViajeId = viajeId;
        ViewBag.OrdenActual = orden;
        return View(envios);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id, int viajeId)
    {
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
        return RedirectToAction("Index", "MisTransportes");
    }
}
