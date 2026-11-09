using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

public class ViajesService : IViajesService
{
    private readonly IViajesRepositorio _repo;
    private readonly IGeocoderService _geocoder;
    private readonly IDistanciaService _distancia;

    public ViajesService(IViajesRepositorio repo, IGeocoderService geocoder, IDistanciaService distancia)
    {
        _repo = repo;
        _geocoder = geocoder;
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

        return ResultadoOperacion<ViajeDto>.Exito(ADto(viaje));
    }

    public async Task<ResultadoOperacion<ViajeDto>> CrearViajeAsync(int usuarioId, CrearViajeDto dto)
    {
        var error = Validar(dto);
        if (error != null)
            return ResultadoOperacion<ViajeDto>.Invalido(error);

        var ruta = await _distancia.CalcularAsync(dto.OrigenDir.Trim(), dto.DestinoDir.Trim());

        var viaje = new Viaje
        {
            UsuarioId = usuarioId,
            OrigenDir = dto.OrigenDir.Trim(),
            OrigenLat = ruta.Origen.Lat,
            OrigenLng = ruta.Origen.Lng,
            DestinoLat = ruta.Destino.Lat,
            DestinoLng = ruta.Destino.Lng,
            HoraSalida = dto.HoraSalida,
            HoraLlegada = dto.HoraLlegada,
            MaxPaquets = dto.MaxPaquetes,
            PesoMaxKg = dto.PesoMaxKg,
            DesvioMaxKm = dto.DesvioMaxKm,
            DistanciaMin = (int)Math.Round(ruta.DistanciaKm),
            Activo = true
        };

        foreach (var dia in dto.Dias.Distinct())
        {
            viaje.ViajeDias.Add(new ViajeDia
            {
                DiaSemana = (byte)dia
            });
        }

        foreach (var p in dto.Paradas.OrderBy(p => p.Orden))
        {
            var coords = await _geocoder.ObtenerCoordenadasAsync(p.Direccion.Trim());
            viaje.Paradas.Add(new Parada
            {
                Orden = p.Orden,
                Direccion = p.Direccion.Trim(),
                Lat = coords.Lat,
                Lng = coords.Lng
            });
        }

        await _repo.CrearAsync(viaje);
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

    public static string? Validar(CrearViajeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OrigenDir) || string.IsNullOrWhiteSpace(dto.DestinoDir))
            return "Origen y destino son obligatorios.";
        if (string.Equals(dto.OrigenDir.Trim(), dto.DestinoDir.Trim(), StringComparison.OrdinalIgnoreCase))
            return "El origen y el destino deben ser diferentes.";
        if (dto.HoraLlegada <= dto.HoraSalida)
            return "La hora de llegada debe ser posterior a la hora de salida.";
        if (dto.Dias == null || dto.Dias.Count == 0)
            return "Debe seleccionar al menos un día.";
        if (dto.MaxPaquetes <= 0)
            return "La cantidad de paquetes debe ser mayor que cero.";
        if (dto.PesoMaxKg <= 0)
            return "El peso máximo debe ser mayor que cero.";

        var direccionesParadas = dto.Paradas.Select(p => p.Direccion.Trim().ToLowerInvariant()).ToList();
        if (direccionesParadas.Distinct().Count() != direccionesParadas.Count)
            return "No se pueden repetir paradas.";

        var origenNorm = dto.OrigenDir.Trim().ToLowerInvariant();
        var destinoNorm = dto.DestinoDir.Trim().ToLowerInvariant();
        if (direccionesParadas.Any(d => d == origenNorm || d == destinoNorm))
            return "Una parada no puede coincidir con el origen o el destino.";

        return null;
    }

    private static ViajeDto ADto(Viaje v) => new()
    {
        Id = v.Id,
        OrigenDir = v.OrigenDir,
        OrigenLat = v.OrigenLat,
        OrigenLng = v.OrigenLng,
        DestinoLat = v.DestinoLat,
        DestinoLng = v.DestinoLng,
        HoraSalida = v.HoraSalida,
        HoraLlegada = v.HoraLlegada,
        MaxPaquetes = v.MaxPaquets,
        PesoMaxKg = v.PesoMaxKg,
        DesvioMaxKm = v.DesvioMaxKm,
        DistanciaKm = v.DistanciaMin,
        Activo = v.Activo,
        Dias = v.ViajeDias.Select(d => (DayOfWeek)d.DiaSemana).ToList(),
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
