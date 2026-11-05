using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
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

        var envios = await _oportunidadesRepositorio.GetEnviosCompatiblesAsync(viaje.PesoMaxKg);

        return orden?.ToLower() switch
        {
            "desvio" => envios.OrderBy(e => e.PesoKg).ToList(),
            "compatibilidad" => envios.OrderByDescending(e => e.Id).ToList(),
            _ => envios.OrderByDescending(e => e.Monto).ToList()
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

        var rutaHabitual = await _distanciaService.CalcularAsync(viaje.OrigenDir, viaje.OrigenDir);
        var rutaConEnvio = await _distanciaService.CalcularAsync(viaje.OrigenDir, envio.DestinoDir);

        decimal distanciaHabitual = (decimal)rutaHabitual.DistanciaKm;
        decimal distanciaConEnvio = (decimal)rutaConEnvio.DistanciaKm;
        decimal desvio = Math.Max(0m, distanciaConEnvio - distanciaHabitual);

        return new DetalleOportunidadDto
        {
            EnvioId = envio.Id,
            ViajeId = viaje.Id,
            Origen = envio.OrigenDir,
            Destino = envio.DestinoDir,
            DistanciaHabitualKm = distanciaHabitual,
            DistanciaConEnvioKm = distanciaConEnvio,
            DesvioKm = Math.Round(desvio, 2),
            TiempoHabitualMin = rutaHabitual.DuracionMinutos,
            TiempoAdicionalMin = 10,
            Pago = envio.Monto,
            PesoKg = envio.PesoKg,
            CapacidadRestanteKg = viaje.PesoMaxKg - envio.PesoKg,
            PaquetesRestantes = viaje.MaxPaquets - 1,
            GananciaPorKmAdicional = desvio > 0 ? Math.Round(envio.Monto / desvio, 2) : envio.Monto
        };
    }

    public async Task<bool> AceptarOportunidadAsync(int envioId, int viajeId)
    {
        return await _oportunidadesRepositorio.ActualizarEstadoEnvioAsync(envioId, 2);
    }
}
