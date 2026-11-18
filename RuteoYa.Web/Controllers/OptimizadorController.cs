using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Web.Models;
using RuteoYa.Web.Services;
using System.Security.Claims;

namespace RuteoYa.Web.Controllers;

[Authorize]
public class OptimizadorController : Controller
{
    private readonly IOptimizacionApiService _apiService;

    public OptimizadorController(IOptimizacionApiService apiService)
    {
        _apiService = apiService;
    }

    // GET: /Optimizador o /Optimizador?viajeId=2 
    public async Task<IActionResult> Index(int? viajeId = null)
    {
        string? nameUsuario = User.FindFirstValue(ClaimTypes.Name);
        ViewData["nombreUsuario"] = nameUsuario ?? "Invitado";
        ViewData["ActivePage"] = "Optimizador";
        var viajesActivos = await _apiService.ObtenerViajesActivosAsync();

        if (!viajesActivos.Any())
        {
            return View(new OptimizadorViewModel
            {
                ViajeId = 0,
                ViajesActivos = viajesActivos
            });
        }

        int idSeleccionado = viajeId ?? viajesActivos.First().Id;
        var (dto, error) = await _apiService.ObtenerOptimizacionAsync(idSeleccionado);

        if (dto == null)
        {
            TempData["Error"] = error ?? "No se pudo obtener la optimización.";
            return View(new OptimizadorViewModel
            {
                ViajeId = idSeleccionado,
                ViajesActivos = viajesActivos
            });
        }

        var vm = new OptimizadorViewModel
        {
            ViajeId = idSeleccionado,
            ViajesActivos = viajesActivos,
            Restricciones = dto.Restricciones,
            Recomendada = dto.CombinacionRecomendada,
            OtrasValidas = dto.OtrasCombinacionesValidas,
            Descartadas = dto.CombinacionesDescartadas
        };

        return View(vm);
    }

    // GET: /Optimizador/Confirmar?viajeId=1 
    public async Task<IActionResult> Confirmar(int viajeId)
    {
        ViewData["ActivePage"] = "Optimizador";

        if (viajeId <= 0)
            return RedirectToAction(nameof(Index));

        var (dto, error) = await _apiService.ObtenerOptimizacionAsync(viajeId);
        var r = dto?.CombinacionRecomendada;

        if (r == null)
        {
            if (!string.IsNullOrEmpty(error))
                TempData["Error"] = error;
            return RedirectToAction(nameof(Index), new { viajeId });
        }

        var vm = new ConfirmarSeleccionViewModel
        {
            ViajeId = viajeId,
            TramoViaje = $"{dto!.Restricciones.Origen} → {dto.Restricciones.Destino}",
            Envios = r.Envios,
            GananciaTotal = r.GananciaTotal,
            PesoTotalKg = r.PesoTotalKg,
            DesvioTotalKm = r.DesvioTotalKm
        };

        return View(vm);
    }

    // POST: /Optimizador/Reservar
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Reservar(int viajeId, List<int> envioIds)
    {
        if (viajeId <= 0 || envioIds == null || !envioIds.Any())
        {
            TempData["Error"] = "Debe seleccionar al menos un envío para reservar.";
            return RedirectToAction(nameof(Index), new { viajeId });
        }

        var (success, message) = await _apiService.ConfirmarCombinacionAsync(viajeId, envioIds);

        if (!success)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index), new { viajeId });
        }

        TempData["Exito"] = $"¡Reserva confirmada con éxito! Se asignaron {envioIds.Count} paquetes.";
        return RedirectToAction(nameof(Index), new { viajeId });
    }
}