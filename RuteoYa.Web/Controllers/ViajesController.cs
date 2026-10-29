using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Web.Models;

namespace RuteoYa.Web.Controllers;

public class ViajesController : Controller
{
    private readonly RuteoYaDbContext _db;
    private readonly IDistanciaService _distanciaService;

    public ViajesController(RuteoYaDbContext db, IDistanciaService distanciaService)
    {
        _db = db;
        _distanciaService = distanciaService;
    }

    public IActionResult Index()
    {
        var viajes = _db.Viajes.ToList();
        return View(viajes);
    }

    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearViajeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // 1. Calcular distancia usando el servicio de Logica
        var ruta = await _distanciaService.CalcularAsync(model.OrigenDir, model.DestinoDir);

        // 2. Crear la entidad con los datos calculados
        var viaje = new Viaje
        {
            UsuarioId = model.UsuarioId,
            OrigenDir = model.OrigenDir,
            OrigenLat = ruta.Origen.Lat,
            OrigenLng = ruta.Origen.Lng,
            DestinoLat = ruta.Destino.Lat,
            DestinoLng = ruta.Destino.Lng,
            HoraSalida = model.HoraSalida,
            HoraLlegada = model.HoraLlegada,
            MaxPaquets = model.MaxPaquetes,
            PesoMaxKg = model.PesoMaxKg,
            DesvioMaxKm = model.DesvioMaxKm,
            DistanciaMin = (int)ruta.DistanciaKm,
            Activo = true
        };

        // 3. Guardar en la base de datos
        _db.Viajes.Add(viaje);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Viaje creado exitosamente. Distancia: {ruta.DistanciaKm:F2} km";
        return RedirectToAction("Index");
    }
}
