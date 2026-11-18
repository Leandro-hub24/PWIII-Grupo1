using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Common.Helpers;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

/// <summary>Mis transportes: el conductor marca retirado / en camino / entregado.</summary>
public class TransportesService : ITransportesService
{
    private readonly IEnviosRepositorio _repo;

    public TransportesService(IEnviosRepositorio repo) => _repo = repo;

    public async Task<List<TransporteDto>> GetMisTransportesAsync(int conductorId, EstadoEnvio? estado)
    {
        var envios = await _repo.GetTransportesDeConductorAsync(conductorId, estado);
        return envios.Select(ADto).ToList();
    }

    public async Task<ResultadoOperacion<TransporteDto>> CambiarEstadoAsync(int conductorId, int envioId, EstadoEnvio nuevo)
    {
        // El conductor solo puede avanzar el paquete; cancelar o asignar no es cosa suya.
        if (nuevo is not (EstadoEnvio.Retirado or EstadoEnvio.EnCamino or EstadoEnvio.Entregado))
            return ResultadoOperacion<TransporteDto>.Invalido("Estado no permitido para un transporte.");

        var envio = await _repo.GetEnvioPorIdAsync(envioId);
        // Si no es conductor de este envío, 404 (igual que en Envíos).
        if (envio?.Asignacion?.Viaje == null || envio.Asignacion.Viaje.UsuarioId != conductorId)
            return ResultadoOperacion<TransporteDto>.NoEncontrado("Transporte no encontrado.");

        var actual = (EstadoEnvio)envio.Estado;
        if (!EstadoEnvioReglas.EsTransicionValida(actual, nuevo))
            return ResultadoOperacion<TransporteDto>.Conflicto(
                $"Transición inválida: de \"{EstadoEnvioReglas.Nombre(actual)}\" a \"{EstadoEnvioReglas.Nombre(nuevo)}\".");

        await _repo.CambiarEstadoAsync(envio, nuevo);
        return ResultadoOperacion<TransporteDto>.Exito(ADto(envio));
    }

    private static EstadoEnvio? Siguiente(EstadoEnvio actual) => actual switch
    {
        EstadoEnvio.ConductorAsignado => EstadoEnvio.Retirado,
        EstadoEnvio.Retirado => EstadoEnvio.EnCamino,
        EstadoEnvio.EnCamino => EstadoEnvio.Entregado,
        _ => null
    };

    private static TransporteDto ADto(Envio e)
    {
        var estado = (EstadoEnvio)e.Estado;
        return new TransporteDto
        {
            EnvioId = e.Id,
            ViajeId = e.Asignacion!.ViajeId,
            ViajeOrigen = e.Asignacion.Viaje?.OrigenDir,
            ViajeDestino = e.Asignacion.Viaje?.DestinoDir,
            FechaViaje = e.Asignacion.FechaViaje,
            Descripcion = e.Descripcion,
            OrigenDir = e.OrigenDir,
            DestinoDir = e.DestinoDir,
            Ganancia = e.Asignacion.Ganancia,
            Estado = estado,
            EstadoNombre = EstadoEnvioReglas.Nombre(estado),
            EstadoSiguiente = Siguiente(estado)
        };
    }
}