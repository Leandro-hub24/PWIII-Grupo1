using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Web.Models;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Controllers;

[Authorize]
public class ViajesController : Controller
{
    private readonly IViajesApiService _viajesApi;

    public ViajesController(IViajesApiService viajesApi)
    {
        _viajesApi = viajesApi;
    }

    public async Task<IActionResult> Index(string filtro = "activos")
    {
        ViewData["ActivePage"] = "Viajes";
        ViewBag.Filtro = filtro;

        bool? activo = true;
        if (filtro == "inactivos") activo = false;
        if (filtro == "todos") activo = null;

        var viajes = await _viajesApi.GetMisViajesAsync(activo);
        return View(viajes);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        ViewData["ActivePage"] = "Viajes";
        var viaje = await _viajesApi.GetViajeAsync(id);
        if (viaje == null) return NotFound();
        return View(viaje);
    }

    public IActionResult Crear()
    {
        ViewData["ActivePage"] = "Viajes";
        return View(new ViajeFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ViajeFormViewModel model)
    {
        ViewData["ActivePage"] = "Viajes";
        Validar(model);
        if (!ModelState.IsValid) return View(model);

        var (viaje, error) = await _viajesApi.CrearViajeAsync(model.ADto());
        if (viaje == null)
        {
            ModelState.AddModelError("", error ?? "No se pudo guardar el viaje.");
            return View(model);
        }

        TempData["Mensaje"] = "Viaje guardado.";
        return RedirectToAction("Detalle", new { id = viaje.Id });
    }

    public async Task<IActionResult> Editar(int id)
    {
        ViewData["ActivePage"] = "Viajes";
        var viaje = await _viajesApi.GetViajeAsync(id);
        if (viaje == null) return NotFound();
        return View(ViajeFormViewModel.DesdeDto(viaje));
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, ViajeFormViewModel model)
    {
        ViewData["ActivePage"] = "Viajes";
        model.Id = id;
        Validar(model);
        if (!ModelState.IsValid) return View(model);

        var (viaje, error) = await _viajesApi.ActualizarViajeAsync(id, model.ADto());
        if (viaje == null)
        {
            ModelState.AddModelError("", error ?? "No se pudo guardar el viaje.");
            return View(model);
        }

        TempData["Mensaje"] = "Viaje actualizado.";
        return RedirectToAction("Detalle", new { id = id });
    }

    [HttpPost]
    public async Task<IActionResult> Desactivar(int id, string volverA)
    {
        var error = await _viajesApi.DesactivarViajeAsync(id);
        if (error == null) TempData["Mensaje"] = "Viaje desactivado.";
        else TempData["Error"] = error;

        if (volverA == "detalle") return RedirectToAction("Detalle", new { id = id });
        return RedirectToAction("Index", new { filtro = "todos" });
    }

    [HttpPost]
    public async Task<IActionResult> Activar(int id, string volverA)
    {
        var error = await _viajesApi.ActivarViajeAsync(id);
        if (error == null) TempData["Mensaje"] = "Viaje activado.";
        else TempData["Error"] = error;

        if (volverA == "detalle") return RedirectToAction("Detalle", new { id = id });
        return RedirectToAction("Index", new { filtro = "todos" });
    }

    [HttpPost]
    public async Task<IActionResult> CalcularRuta(ViajeFormViewModel model)
    {
        ViewData["ActivePage"] = "Viajes";
        var calcular = new CalcularRutaDto();
        calcular.OrigenDir = model.OrigenDir;
        calcular.DestinoDir = model.DestinoDir;
        calcular.Paradas = model.Paradas.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

        var (ruta, error) = await _viajesApi.CalcularRutaAsync(calcular);
        if (ruta == null) ViewBag.RutaError = error;
        else ViewBag.Ruta = ruta;

        ModelState.Clear();
        return View(model.Id == null ? "Crear" : "Editar", model);
    }

    private void Validar(ViajeFormViewModel model)
    {
        if (model.Dias.Count == 0)
            ModelState.AddModelError("Dias", "Seleccioná al menos un día.");

        if (model.OrigenDir != null && model.DestinoDir != null &&
            model.OrigenDir.Trim().ToLower() == model.DestinoDir.Trim().ToLower())
            ModelState.AddModelError("DestinoDir", "El destino debe ser diferente al origen.");

        if (model.HoraSalida != null && model.HoraLlegada != null && model.HoraLlegada <= model.HoraSalida)
            ModelState.AddModelError("HoraLlegada", "La hora de llegada debe ser posterior a la de salida.");
    }
}
