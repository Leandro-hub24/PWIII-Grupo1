
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Web.Services;

namespace PrototipoDeRuteaYa.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        private readonly ITransportesApiService _transportesApiService;

        public HistorialController(ITransportesApiService transportesApiService)
        {
            _transportesApiService = transportesApiService;
        }

        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
        {
            var transportes = await _transportesApiService
                .GetMisTransportesAsync(EstadoEnvio.Entregado);

            if (desde.HasValue)
                transportes = transportes
                    .Where(t => t.FechaViaje.Date >= desde.Value.Date)
                    .ToList();

            if (hasta.HasValue)
                transportes = transportes
                    .Where(t => t.FechaViaje.Date <= hasta.Value.Date)
                    .ToList();

            transportes = transportes
                .OrderByDescending(t => t.FechaViaje)
                .ToList();

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.GananciaTotal = transportes.Sum(t => t.Ganancia);

            return View(transportes);
        }
    }
}
