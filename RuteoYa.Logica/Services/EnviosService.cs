using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Common.Helpers;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

/// <summary>
/// Reglas de negocio de los envíos: publicar, listar, ver detalle, cancelar y cambiar estado.
/// </summary>
public class EnviosService : IEnviosService
{
    private readonly IEnviosRepositorio _repo;
    private readonly IGeocoderService _geocoder;

    public EnviosService(IEnviosRepositorio repo, IGeocoderService geocoder)
    {
        _repo = repo;
        _geocoder = geocoder;
    }

    public async Task<List<EnvioDto>> GetMisEnviosAsync(int usuarioId, EstadoEnvio? estado)
    {
        var envios = await _repo.GetEnviosDeUsuarioAsync(usuarioId, estado);
        return envios.Select(ADto).ToList();
    }

    public async Task<ResultadoOperacion<EnvioDto>> GetEnvioAsync(int usuarioId, int envioId)
    {
        var envio = await _repo.GetEnvioPorIdAsync(envioId);
        // Si el envío es de otro usuario respondemos 404, para no revelar que existe.
        if (envio == null || envio.UsuarioId != usuarioId)
            return ResultadoOperacion<EnvioDto>.NoEncontrado("Envío no encontrado.");

        return ResultadoOperacion<EnvioDto>.Exito(ADto(envio));
    }

    public async Task<ResultadoOperacion<EnvioDto>> CrearEnvioAsync(int usuarioId, CrearEnvioDto dto)
    {
        var error = Validar(dto);
        if (error != null)
            return ResultadoOperacion<EnvioDto>.Invalido(error);

        var origen = await _geocoder.ObtenerCoordenadasAsync(dto.OrigenDir.Trim());
        var destino = await _geocoder.ObtenerCoordenadasAsync(dto.DestinoDir.Trim());
        if (origen == null || destino == null)
            return ResultadoOperacion<EnvioDto>.Invalido("No se pudo ubicar el origen o el destino. Revisá las direcciones.");

        var envio = new Envio
        {
            UsuarioId = usuarioId,
            OrigenDir = dto.OrigenDir.Trim(),
            OrigenLat = origen.Lat,
            OrigenLng = origen.Lng,
            DestinoDir = dto.DestinoDir.Trim(),
            DestinoLat = destino.Lat,
            DestinoLng = destino.Lng,
            FechaLimite = dto.Fecha.Date,
            PesoKg = dto.PesoKg,
            Tamano = (byte)dto.Tamano!.Value,
            Descripcion = dto.Descripcion.Trim(),
            Monto = dto.Monto,
            Estado = (byte)EstadoEnvio.BuscandoConductor,
            CreadoEn = DateTime.Now
        };

        await _repo.CrearAsync(envio);
        return ResultadoOperacion<EnvioDto>.Exito(ADto(envio));
    }

    public async Task<ResultadoOperacion<EnvioDto>> CancelarEnvioAsync(int usuarioId, int envioId)
    {
        var envio = await _repo.GetEnvioPorIdAsync(envioId);
        if (envio == null || envio.UsuarioId != usuarioId)
            return ResultadoOperacion<EnvioDto>.NoEncontrado("Envío no encontrado.");

        var actual = (EstadoEnvio)envio.Estado;
        if (!EstadoEnvioReglas.SePuedeCancelar(actual))
            return ResultadoOperacion<EnvioDto>.Conflicto(
                $"No se puede cancelar un envío en estado \"{EstadoEnvioReglas.Nombre(actual)}\".");

        await _repo.CambiarEstadoAsync(envio, EstadoEnvio.Cancelado);
        return ResultadoOperacion<EnvioDto>.Exito(ADto(envio));
    }

    public async Task<ResultadoOperacion<EnvioDto>> CambiarEstadoAsync(int envioId, EstadoEnvio nuevo)
    {
        var envio = await _repo.GetEnvioPorIdAsync(envioId);
        if (envio == null)
            return ResultadoOperacion<EnvioDto>.NoEncontrado("Envío no encontrado.");

        var actual = (EstadoEnvio)envio.Estado;
        if (!EstadoEnvioReglas.EsTransicionValida(actual, nuevo))
            return ResultadoOperacion<EnvioDto>.Conflicto(
                $"Transición inválida: de \"{EstadoEnvioReglas.Nombre(actual)}\" a \"{EstadoEnvioReglas.Nombre(nuevo)}\".");

        await _repo.CambiarEstadoAsync(envio, nuevo);
        return ResultadoOperacion<EnvioDto>.Exito(ADto(envio));
    }

    /// <summary>Validaciones de la Pantalla 6 que no cubren los DataAnnotations.</summary>
    public static string? Validar(CrearEnvioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OrigenDir) || string.IsNullOrWhiteSpace(dto.DestinoDir))
            return "Origen y destino son obligatorios.";
        if (string.Equals(dto.OrigenDir.Trim(), dto.DestinoDir.Trim(), StringComparison.OrdinalIgnoreCase))
            return "El origen y el destino deben ser diferentes.";
        if (dto.Fecha.Date < DateTime.Today)
            return "La fecha no puede ser anterior a hoy.";
        if (dto.PesoKg <= 0)
            return "El peso debe ser mayor que cero.";
        if (dto.Tamano == null || !Enum.IsDefined(dto.Tamano.Value))
            return "El tamaño es obligatorio.";
        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            return "La descripción es obligatoria.";
        if (dto.Monto <= 0)
            return "El monto ofrecido debe ser mayor que cero.";
        return null;
    }

    private static EnvioDto ADto(Envio e)
    {
        var estado = (EstadoEnvio)e.Estado;
        return new EnvioDto
        {
            Id = e.Id,
            OrigenDir = e.OrigenDir,
            DestinoDir = e.DestinoDir,
            Fecha = e.FechaLimite,
            PesoKg = e.PesoKg,
            Tamano = (TamanoEnvio)e.Tamano,
            Descripcion = e.Descripcion,
            Monto = e.Monto,
            Estado = estado,
            EstadoNombre = EstadoEnvioReglas.Nombre(estado),
            SePuedeCancelar = EstadoEnvioReglas.SePuedeCancelar(estado),
            CreadoEn = e.CreadoEn,
            ViajeId = e.Asignacion?.ViajeId,
            ViajeOrigen = e.Asignacion?.Viaje?.OrigenDir,
            FechaViaje = e.Asignacion?.FechaViaje,
            Historial = e.EnvioEstadoHistorial
                .OrderBy(h => h.Fecha)
                .Select(h => new EnvioEstadoHistorialDto
                {
                    EstadoAnterior = EstadoEnvioReglas.Nombre((EstadoEnvio)h.EstadoAnterior),
                    EstadoNuevo = EstadoEnvioReglas.Nombre((EstadoEnvio)h.EstadoNuevo),
                    Fecha = h.Fecha
                }).ToList()
        };
    }
}
