using RuteoYa.Entidades;
using RuteoYa.Web.Models;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Logic;

/// <summary>
/// Orquestador de lógica de presentación para Viajes.
/// Coordina llamadas a la API y transforma datos entre ViewModels y Entidades.
/// </summary>
public class ViajesOrchestrator
{
    private readonly RuteoYaDbContext _db;
    private readonly IViajesApiService _viajesApiService;

    public ViajesOrchestrator(RuteoYaDbContext db, IViajesApiService viajesApiService)
    {
        _db = db;
        _viajesApiService = viajesApiService;
    }

    /// <summary>
    /// Crea un viaje calculando automáticamente las coordenadas y distancias.
    /// </summary>
    public async Task<(bool Success, string Message, int? ViajeId)> CrearViajeAsync(CrearViajeViewModel model)
    {
        try
        {
            // 1. Calcular ruta y coordenadas vía API
            var ruta = await _viajesApiService.CalcularDistanciaAsync(model.OrigenDir, model.DestinoDir);

            if (ruta == null)
            {
                return (false, "No se pudo calcular la ruta. Verifique las direcciones.", null);
            }

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
}
