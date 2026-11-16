using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Controllers;

/// <summary>
/// Pantalla 6 (Nuevo envío) y Pantalla 7 (Mis envíos + detalle).
/// </summary>
[Authorize]
public class EnviosController : Controller
{
    private readonly IEnviosApiService _enviosApi;

    public EnviosController(IEnviosApiService enviosApi)
    {
        _enviosApi = enviosApi;
    }

    // GET: /Envios?estado=1
    public async Task<IActionResult> Index(EstadoEnvio? estado)
    {
        ViewData["ActivePage"] = "Envios";
        ViewBag.EstadoActual = estado;
        var envios = await _enviosApi.GetMisEnviosAsync(estado);
        return View(envios);
    }

    // GET: /Envios/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        ViewData["ActivePage"] = "Envios";
        var envio = await _enviosApi.GetEnvioAsync(id);
        if (envio == null) return NotFound();
        return View(envio);
    }

    // GET: /Envios/Crear
    public IActionResult Crear()
    {
        ViewData["ActivePage"] = "Envios";
        return View(new CrearEnvioDto { Fecha = DateTime.Today });
    }

    // POST: /Envios/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearEnvioDto model)
    {
        ViewData["ActivePage"] = "Envios";

        // Validaciones de la interfaz (la API vuelve a validar todo).
        if (!string.IsNullOrWhiteSpace(model.OrigenDir) &&
            string.Equals(model.OrigenDir.Trim(), model.DestinoDir?.Trim(), StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.DestinoDir), "El destino debe ser diferente al origen.");
        if (model.Fecha.Date < DateTime.Today)
            ModelState.AddModelError(nameof(model.Fecha), "La fecha no puede ser anterior a hoy.");

        if (!ModelState.IsValid) return View(model);

        var (envio, error) = await _enviosApi.CrearEnvioAsync(model);
        if (envio == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo publicar el envío.");
            return View(model);
        }

        TempData["Mensaje"] = "Envío publicado. Ya está buscando conductor.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Envios/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var error = await _enviosApi.CancelarEnvioAsync(id);
        if (error == null) TempData["Mensaje"] = "Envío cancelado.";
        else TempData["Error"] = error;
        return RedirectToAction(nameof(Detalle), new { id });
    }
}
