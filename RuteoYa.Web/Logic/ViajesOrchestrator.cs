using RuteoYa.Entidades;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Web.Models;

namespace RuteoYa.Web.Logic;

/// <summary>
/// Orquestador de lógica de presentación para Viajes.
/// Coordina múltiples servicios de negocio y transforma datos
/// entre ViewModels y Entidades.
/// </summary>
public class ViajesOrchestrator
{
    private readonly RuteoYaDbContext _db;
    private readonly IDistanciaService _distanciaService;
    private readonly IGeocoderService _geocoderService;

    public ViajesOrchestrator(
        RuteoYaDbContext db,
        IDistanciaService distanciaService,
        IGeocoderService geocoderService)
    {
        _db = db;
        _distanciaService = distanciaService;
        _geocoderService = geocoderService;
    }

    /// <summary>
    /// Crea un viaje calculando automáticamente las coordenadas y distancias.
    /// </summary>
    public async Task<(bool Success, string Message, int? ViajeId)> CrearViajeAsync(CrearViajeViewModel model)
    {
        try
        {
            // 1. Calcular ruta y coordenadas
            var ruta = await _distanciaService.CalcularAsync(model.OrigenDir, model.DestinoDir);

            // 2. Validaciones de negocio específicas de la UI
            if (ruta.DistanciaKm > 500)
            {
                return (false, "La distancia entre origen y destino no puede superar 500 km", null);
            }

            if (model.HoraLlegada <= model.HoraSalida)
            {
                return (false, "La hora de llegada debe ser posterior a la hora de salida", null);
            }

            // 3. Crear la entidad
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
                DistanciaMin = (int)Math.Round(ruta.DistanciaKm),
                Activo = true
            };

            // 4. Guardar
            _db.Viajes.Add(viaje);
            await _db.SaveChangesAsync();

            return (true, $"Viaje creado exitosamente. Distancia: {ruta.DistanciaKm:F2} km", viaje.Id);
        }
        catch (Exception ex)
        {
            // Log del error (inyectar ILogger si es necesario)
            return (false, $"Error al crear el viaje: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Valida si una dirección es geocodificable antes de crear el viaje.
    /// </summary>
    public async Task<bool> ValidarDireccionAsync(string direccion)
    {
        try
        {
            var resultado = await _geocoderService.GeocodeAsync(direccion);
            return resultado.Lat != 0 && resultado.Lng != 0;
        }
        catch
        {
            return false;
        }
    }
}
