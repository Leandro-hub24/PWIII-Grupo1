
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrototipoDeRuteaYa.Models;
using RuteoYa.Web.Services;
using RuteoYa.Web.Logic;
using RuteoYa.Entidades.Common.Enums;
using System.Diagnostics;
using System.Security.Claims;

namespace PrototipoDeRuteaYa.Controllers
{
    public class HomeController : Controller
    {
        private readonly IViajesApiService _viajesApiService;
        private readonly IEnviosApiService _enviosApiService;
        private readonly OportunidadesOrchestrator _oportunidadesOrchestrator;

        public HomeController(
            IViajesApiService viajesApiService,
            IEnviosApiService enviosApiService,
            OportunidadesOrchestrator oportunidadesOrchestrator)
        {
            _viajesApiService = viajesApiService;
            _enviosApiService = enviosApiService;
            _oportunidadesOrchestrator = oportunidadesOrchestrator;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Home";

            string? nameUsuario = User.FindFirstValue(ClaimTypes.Name);
            ViewData["nombreUsuario"] = nameUsuario ?? "Invitado";

            // Viajes activos
            var viajesActivos = await _viajesApiService.GetMisViajesAsync(true);

            ViewData["CantidadViajes"] = viajesActivos.Count;
            ViewData["ViajesActivos"] = viajesActivos;

            // Envíos pendientes
            var todosLosEnvios = await _enviosApiService.GetMisEnviosAsync(null);

            var enviosPendientes = todosLosEnvios
                .Where(e => e.Estado == EstadoEnvio.BuscandoConductor ||
                            e.Estado == EstadoEnvio.ConductorAsignado)
                .ToList();

            ViewData["CantidadEnviosPendientes"] = enviosPendientes.Count;
            ViewData["EnviosPendientes"] = enviosPendientes;

            // Oportunidades compatibles con los viajes activos
            var oportunidadesIds = new HashSet<int>();

            foreach (var viaje in viajesActivos)
            {
                var oportunidades = await _oportunidadesOrchestrator
                    .ObtenerOportunidadesAsync(viaje.Id, "ganancia", 1 , 50);

                foreach (var envio in oportunidades.Items)
                {
                    oportunidadesIds.Add(envio.Id);
                }
            }

            ViewData["CantidadOportunidades"] = oportunidadesIds.Count;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
