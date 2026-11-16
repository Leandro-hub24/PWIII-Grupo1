using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Enums;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

public class OportunidadesService : IOportunidadesService
{
    private readonly IOportunidadesRepositorio _oportunidadesRepositorio;
    private readonly IDistanciaService _distanciaService;

    public OportunidadesService(IOportunidadesRepositorio oportunidadesRepositorio, IDistanciaService distanciaService)
    {
        _oportunidadesRepositorio = oportunidadesRepositorio;
        _distanciaService = distanciaService;
    }

    public async Task<Viaje?> ObtenerViajePorIdAsync(int viajeId)
    {
        return await _oportunidadesRepositorio.GetViajePorIdAsync(viajeId);
    }

    public async Task<List<Envio>> ObtenerEnviosCompatiblesAsync(int viajeId, string? orden)
    {
        var viaje = await _oportunidadesRepositorio.GetViajePorIdAsync(viajeId);
        if (viaje == null) return new List<Envio>();

        // Capacidad que le queda al viaje
        var (paquetesAsignados, pesoAsignado) = await _oportunidadesRepositorio.GetCargaAsignadaAsync(viajeId);
        if (paquetesAsignados >= viaje.MaxPaquets) return new List<Envio>();

        var pesoDisponible = viaje.PesoMaxKg - pesoAsignado;
        if (pesoDisponible <= 0) return new List<Envio>();

        var candidatos = await _oportunidadesRepositorio.GetEnviosCompatiblesAsync(pesoDisponible);

        // Ruta habitual: se calcula una sola vez
        var rutaHabitual = await CalcularRutaHabitualAsync(viaje);
        decimal distanciaHabitual = (decimal)rutaHabitual.DistanciaKm;

        var compatibles = new List<(Envio Envio, decimal Desvio)>();

        foreach (var envio in candidatos)
        {
            // No se puede aceptar un envío propio
            if (envio.UsuarioId == viaje.UsuarioId) continue;

            var (distanciaConEnvio, _) = await CalcularRutaConEnvioAsync(viaje, envio);
            decimal desvio = Math.Max(0m, distanciaConEnvio - distanciaHabitual);

            if (desvio <= viaje.DesvioMaxKm)
            {
                compatibles.Add((envio, desvio));
            }
        }

        return orden?.ToLower() switch
        {
            "desvio" => compatibles.OrderBy(x => x.Desvio).Select(x => x.Envio).ToList(),
            "compatibilidad" => compatibles
                .OrderByDescending(x => x.Desvio > 0 ? x.Envio.Monto / x.Desvio : x.Envio.Monto)
                .Select(x => x.Envio).ToList(),
            _ => compatibles.OrderByDescending(x => x.Envio.Monto).Select(x => x.Envio).ToList()
        };
    }

    public async Task<Envio?> ObtenerEnvioPorIdAsync(int envioId)
    {
        return await _oportunidadesRepositorio.GetEnvioPorIdAsync(envioId);
    }

    public async Task<DetalleOportunidadDto?> CalcularDetalleOportunidadAsync(int viajeId, int envioId)
    {
        var viaje = await _oportunidadesRepositorio.GetViajePorIdAsync(viajeId);
        var envio = await _oportunidadesRepositorio.GetEnvioPorIdAsync(envioId);

        if (viaje == null || envio == null) return null;

        var rutaHabitual = await CalcularRutaHabitualAsync(viaje);
        var (distanciaConEnvio, duracionConEnvio) = await CalcularRutaConEnvioAsync(viaje, envio);
        var (paquetesAsignados, pesoAsignado) = await _oportunidadesRepositorio.GetCargaAsignadaAsync(viajeId);

        decimal distanciaHabitual = (decimal)rutaHabitual.DistanciaKm;
        decimal desvio = Math.Max(0m, distanciaConEnvio - distanciaHabitual);

        return new DetalleOportunidadDto
        {
            EnvioId = envio.Id,
            ViajeId = viaje.Id,
            Origen = envio.OrigenDir,
            Destino = envio.DestinoDir,
            DistanciaHabitualKm = Math.Round(distanciaHabitual, 2),
            DistanciaConEnvioKm = Math.Round(distanciaConEnvio, 2),
            DesvioKm = Math.Round(desvio, 2),
            TiempoHabitualMin = rutaHabitual.DuracionMinutos,
            TiempoAdicionalMin = Math.Max(0, duracionConEnvio - rutaHabitual.DuracionMinutos),
            Pago = envio.Monto,
            PesoKg = envio.PesoKg,
            CapacidadRestanteKg = viaje.PesoMaxKg - pesoAsignado - envio.PesoKg,
            PaquetesRestantes = viaje.MaxPaquets - paquetesAsignados - 1,
            GananciaPorKmAdicional = desvio > 0 ? Math.Round(envio.Monto / desvio, 2) : envio.Monto
        };
    }

    public async Task<ResultadoOperacion> AceptarOportunidadAsync(int envioId, int viajeId)
    {
        var viaje = await _oportunidadesRepositorio.GetViajePorIdAsync(viajeId);
        if (viaje == null)
            return new ResultadoOperacion(false, "El viaje no existe o no está activo.");

        var envio = await _oportunidadesRepositorio.GetEnvioPorIdAsync(envioId);
        if (envio == null)
            return new ResultadoOperacion(false, "El envío no existe.");

        if (envio.Estado != (byte)EstadoEnvio.Pendiente)
            return new ResultadoOperacion(false, "El envío ya no está disponible.");

        if (envio.UsuarioId == viaje.UsuarioId)
            return new ResultadoOperacion(false, "No podés aceptar tu propio envío.");

        var (paquetesAsignados, pesoAsignado) = await _oportunidadesRepositorio.GetCargaAsignadaAsync(viajeId);

        if (paquetesAsignados >= viaje.MaxPaquets)
            return new ResultadoOperacion(false, "El viaje ya alcanzó el máximo de paquetes.");

        if (pesoAsignado + envio.PesoKg > viaje.PesoMaxKg)
            return new ResultadoOperacion(false, "El envío supera la capacidad de peso restante del viaje.");

        var rutaHabitual = await CalcularRutaHabitualAsync(viaje);
        var (distanciaConEnvio, duracionConEnvio) = await CalcularRutaConEnvioAsync(viaje, envio);

        decimal desvio = Math.Max(0m, distanciaConEnvio - (decimal)rutaHabitual.DistanciaKm);
        if (desvio > viaje.DesvioMaxKm)
            return new ResultadoOperacion(false, "El desvío de este envío supera el máximo permitido para tu viaje.");

        // Llamada al repositorio pasando el ID, el viaje y el estado en formato byte
        var asignado = await _oportunidadesRepositorio.AsignarEnvioAsync(envio.Id, viajeId, (byte)EstadoEnvio.Asignado);

        return asignado
            ? new ResultadoOperacion(true, "Envío aceptado exitosamente.")
            : new ResultadoOperacion(false, "El envío ya no está disponible, otro conductor lo aceptó antes.");
    }
    

    private Task<DistanciaRutaDto> CalcularRutaHabitualAsync(Viaje viaje)
    {
        return _distanciaService.CalcularPorCoordenadasAsync(
            (double)viaje.OrigenLat, (double)viaje.OrigenLng,
            (double)viaje.DestinoLat, (double)viaje.DestinoLng);
    }

    
    private async Task<(decimal DistanciaKm, int DuracionMin)> CalcularRutaConEnvioAsync(Viaje viaje, Envio envio)
    {
        var tramo1 = await _distanciaService.CalcularPorCoordenadasAsync(
            (double)viaje.OrigenLat, (double)viaje.OrigenLng,
            (double)envio.OrigenLat, (double)envio.OrigenLng);

        var tramo2 = await _distanciaService.CalcularPorCoordenadasAsync(
            (double)envio.OrigenLat, (double)envio.OrigenLng,
            (double)envio.DestinoLat, (double)envio.DestinoLng);

        var tramo3 = await _distanciaService.CalcularPorCoordenadasAsync(
            (double)envio.DestinoLat, (double)envio.DestinoLng,
            (double)viaje.DestinoLat, (double)viaje.DestinoLng);

        decimal distanciaTotal = (decimal)(tramo1.DistanciaKm + tramo2.DistanciaKm + tramo3.DistanciaKm);
        int duracionTotal = tramo1.DuracionMinutos + tramo2.DuracionMinutos + tramo3.DuracionMinutos;

        return (distanciaTotal, duracionTotal);
    }

    
}