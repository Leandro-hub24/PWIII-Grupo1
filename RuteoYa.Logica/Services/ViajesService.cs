using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Common.Helpers;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

public class ViajesService : IViajesService
{
    private readonly IViajesRepositorio _repo;
    private readonly IDistanciaService _distancia;

    public ViajesService(IViajesRepositorio repo, IDistanciaService distancia)
    {
        _repo = repo;
        _distancia = distancia;
    }

    public async Task<List<ViajeDto>> GetMisViajesAsync(int usuarioId, bool? activo)
    {
        var viajes = await _repo.GetViajesDeUsuarioAsync(usuarioId, activo);
        return viajes.Select(ADto).ToList();
    }

    public async Task<ResultadoOperacion<ViajeDto>> GetViajeAsync(int usuarioId, int viajeId)
    {
        var viaje = await _repo.GetViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return ResultadoOperacion<ViajeDto>.NoEncontrado("Viaje no encontrado.");

        var dto = ADto(viaje);
        var envios = await _repo.GetEnviosAsignadosAsync(viajeId);
        dto.PaquetesAsignados = envios.Select(e => new PaqueteAsignadoDto
        {
            EnvioId = e.Id,
            Descripcion = e.Descripcion,
            OrigenDir = e.OrigenDir,
            DestinoDir = e.DestinoDir,
            PesoKg = e.PesoKg,
            Monto = e.Monto,
            Estado = EstadoEnvioReglas.Nombre((EstadoEnvio)e.Estado),
            FechaViaje = e.Asignacion!.FechaViaje
        }).ToList();

        return ResultadoOperacion<ViajeDto>.Exito(dto);
    }

    public async Task<ResultadoOperacion<ViajeDto>> CrearViajeAsync(int usuarioId, CrearViajeDto dto)
    {
        var error = Validar(dto);
        if (error != null)
            return ResultadoOperacion<ViajeDto>.Invalido(error);

        var viaje = new Viaje { UsuarioId = usuarioId, Activo = true };
        await CargarDatosAsync(viaje, dto);

        await _repo.CrearAsync(viaje);
        return ResultadoOperacion<ViajeDto>.Exito(ADto(viaje));
    }

    public async Task<ResultadoOperacion<ViajeDto>> ActualizarViajeAsync(int usuarioId, int viajeId, CrearViajeDto dto)
    {
        var viaje = await _repo.GetViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return ResultadoOperacion<ViajeDto>.NoEncontrado("Viaje no encontrado.");

        var error = Validar(dto);
        if (error != null)
            return ResultadoOperacion<ViajeDto>.Invalido(error);

        viaje.Paradas.Clear();
        viaje.ViajeDias.Clear();
        await CargarDatosAsync(viaje, dto);

        await _repo.ActualizarAsync(viaje);
        return ResultadoOperacion<ViajeDto>.Exito(ADto(viaje));
    }

    public async Task<ResultadoOperacion<ViajeDto>> DesactivarViajeAsync(int usuarioId, int viajeId)
    {
        var viaje = await _repo.GetViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return ResultadoOperacion<ViajeDto>.NoEncontrado("Viaje no encontrado.");

        if (!viaje.Activo)
            return ResultadoOperacion<ViajeDto>.Conflicto("El viaje ya está inactivo.");

        viaje.Activo = false;
        await _repo.ActualizarAsync(viaje);
        return ResultadoOperacion<ViajeDto>.Exito(ADto(viaje));
    }

    public async Task<ResultadoOperacion<ViajeDto>> ActivarViajeAsync(int usuarioId, int viajeId)
    {
        var viaje = await _repo.GetViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return ResultadoOperacion<ViajeDto>.NoEncontrado("Viaje no encontrado.");

        if (viaje.Activo)
            return ResultadoOperacion<ViajeDto>.Conflicto("El viaje ya está activo.");

        viaje.Activo = true;
        await _repo.ActualizarAsync(viaje);
        return ResultadoOperacion<ViajeDto>.Exito(ADto(viaje));
    }

    public async Task<ResultadoOperacion<RutaViajeDto>> CalcularRutaAsync(CalcularRutaDto dto)
    {
        var paradas = (dto.Paradas ?? new List<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .ToList();

        var error = ValidarRecorrido(dto.OrigenDir, dto.DestinoDir, paradas);
        if (error != null)
            return ResultadoOperacion<RutaViajeDto>.Invalido(error);

        var ruta = await CalcularRutaAsync(dto.OrigenDir.Trim(), dto.DestinoDir.Trim(), paradas);
        return ResultadoOperacion<RutaViajeDto>.Exito(ruta);
    }

    public static string? Validar(CrearViajeDto dto)
    {
        var paradas = dto.Paradas ?? new List<CrearParadaDto>();
        if (paradas.Any(p => string.IsNullOrWhiteSpace(p.Direccion)))
            return "Las paradas deben tener una dirección.";

        var error = ValidarRecorrido(dto.OrigenDir, dto.DestinoDir, paradas.Select(p => p.Direccion.Trim()).ToList());
        if (error != null)
            return error;

        if (paradas.Select(p => p.Orden).Distinct().Count() != paradas.Count)
            return "Las paradas no pueden repetir el número de orden.";
        if (dto.HoraLlegada <= dto.HoraSalida)
            return "La hora de llegada debe ser posterior a la hora de salida.";
        if (dto.Dias == null || dto.Dias.Count == 0)
            return "Debe seleccionar al menos un día.";
        if (dto.MaxPaquetes <= 0)
            return "La cantidad de paquetes debe ser mayor que cero.";
        if (dto.PesoMaxKg <= 0)
            return "El peso máximo debe ser mayor que cero.";
        if (dto.DesvioMaxKm < 0)
            return "El desvío máximo no puede ser negativo.";

        return null;
    }

    private static string? ValidarRecorrido(string? origen, string? destino, List<string> paradas)
    {
        if (string.IsNullOrWhiteSpace(origen) || string.IsNullOrWhiteSpace(destino))
            return "Origen y destino son obligatorios.";

        var origenNorm = origen.Trim().ToLowerInvariant();
        var destinoNorm = destino.Trim().ToLowerInvariant();
        if (origenNorm == destinoNorm)
            return "El origen y el destino deben ser diferentes.";

        var paradasNorm = paradas.Select(p => p.ToLowerInvariant()).ToList();
        if (paradasNorm.Distinct().Count() != paradasNorm.Count)
            return "No se pueden repetir paradas.";
        if (paradasNorm.Any(p => p == origenNorm || p == destinoNorm))
            return "Una parada no puede coincidir con el origen o el destino.";

        return null;
    }

    private async Task CargarDatosAsync(Viaje viaje, CrearViajeDto dto)
    {
        var paradas = (dto.Paradas ?? new List<CrearParadaDto>())
            .OrderBy(p => p.Orden)
            .Select(p => p.Direccion.Trim())
            .ToList();

        var ruta = await CalcularRutaAsync(dto.OrigenDir.Trim(), dto.DestinoDir.Trim(), paradas);
        var origen = ruta.Puntos.First();
        var destino = ruta.Puntos.Last();

        viaje.OrigenDir = dto.OrigenDir.Trim();
        viaje.OrigenLat = origen.Lat;
        viaje.OrigenLng = origen.Lng;
        viaje.DestinoDir = dto.DestinoDir.Trim();
        viaje.DestinoLat = destino.Lat;
        viaje.DestinoLng = destino.Lng;
        viaje.HoraSalida = dto.HoraSalida;
        viaje.HoraLlegada = dto.HoraLlegada;
        viaje.MaxPaquets = dto.MaxPaquetes;
        viaje.PesoMaxKg = dto.PesoMaxKg;
        viaje.DesvioMaxKm = dto.DesvioMaxKm;
        viaje.DistanciaMin = (int)Math.Round(ruta.DistanciaKm);

        foreach (var dia in dto.Dias.Distinct().OrderBy(d => d))
            viaje.ViajeDias.Add(new ViajeDia { DiaSemana = (byte)dia });

        for (var i = 0; i < paradas.Count; i++)
        {
            var punto = ruta.Puntos[i + 1];
            viaje.Paradas.Add(new Parada
            {
                Orden = i + 1,
                Direccion = paradas[i],
                Lat = punto.Lat,
                Lng = punto.Lng
            });
        }
    }

    private async Task<RutaViajeDto> CalcularRutaAsync(string origen, string destino, List<string> paradas)
    {
        var direcciones = new List<string> { origen };
        direcciones.AddRange(paradas);
        direcciones.Add(destino);

        var ruta = new RutaViajeDto();
        for (var i = 0; i < direcciones.Count - 1; i++)
        {
            var tramo = await _distancia.CalcularAsync(direcciones[i], direcciones[i + 1]);
            ruta.DistanciaKm += tramo.DistanciaKm;
            ruta.DuracionMinutos += tramo.DuracionMinutos;

            if (i == 0)
                ruta.Puntos.Add(new GeocoderDto { Direccion = direcciones[0], Lat = tramo.Origen.Lat, Lng = tramo.Origen.Lng });
            ruta.Puntos.Add(new GeocoderDto { Direccion = direcciones[i + 1], Lat = tramo.Destino.Lat, Lng = tramo.Destino.Lng });
        }

        ruta.DistanciaKm = Math.Round(ruta.DistanciaKm, 2);
        return ruta;
    }

    private static ViajeDto ADto(Viaje v) => new()
    {
        Id = v.Id,
        OrigenDir = v.OrigenDir,
        OrigenLat = v.OrigenLat,
        OrigenLng = v.OrigenLng,
        DestinoDir = v.DestinoDir,
        DestinoLat = v.DestinoLat,
        DestinoLng = v.DestinoLng,
        HoraSalida = v.HoraSalida,
        HoraLlegada = v.HoraLlegada,
        MaxPaquetes = v.MaxPaquets,
        PesoMaxKg = v.PesoMaxKg,
        DesvioMaxKm = v.DesvioMaxKm,
        DistanciaKm = v.DistanciaMin,
        Activo = v.Activo,
        Dias = v.ViajeDias.Select(d => (DayOfWeek)d.DiaSemana).OrderBy(d => d).ToList(),
        Paradas = v.Paradas.OrderBy(p => p.Orden).Select(p => new ParadaDto
        {
            Id = p.Id,
            Orden = p.Orden,
            Direccion = p.Direccion,
            Lat = p.Lat,
            Lng = p.Lng
        }).ToList()
    };
}
