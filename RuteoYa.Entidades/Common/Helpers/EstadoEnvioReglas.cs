using RuteoYa.Entidades.Common.Enums;

namespace RuteoYa.Entidades.Common.Helpers;

/// <summary>
/// Transiciones de estado válidas de un envío (punto 5 del enunciado).
/// La usan Envíos (cancelar) y Mis transportes (retirado / en camino / entregado).
/// </summary>
public static class EstadoEnvioReglas
{
    private static readonly Dictionary<EstadoEnvio, EstadoEnvio[]> Transiciones = new()
    {
        [EstadoEnvio.BuscandoConductor] = new[] { EstadoEnvio.ConductorAsignado, EstadoEnvio.Cancelado },
        [EstadoEnvio.ConductorAsignado] = new[] { EstadoEnvio.Retirado, EstadoEnvio.Cancelado },
        [EstadoEnvio.Retirado] = new[] { EstadoEnvio.EnCamino },
        [EstadoEnvio.EnCamino] = new[] { EstadoEnvio.Entregado },
        [EstadoEnvio.Entregado] = Array.Empty<EstadoEnvio>(),
        [EstadoEnvio.Cancelado] = Array.Empty<EstadoEnvio>()
    };

    public static bool EsTransicionValida(EstadoEnvio actual, EstadoEnvio nuevo)
        => Transiciones.TryGetValue(actual, out var posibles) && posibles.Contains(nuevo);

    /// <summary>El remitente solo puede cancelar antes de que retiren el paquete.</summary>
    public static bool SePuedeCancelar(EstadoEnvio actual)
        => EsTransicionValida(actual, EstadoEnvio.Cancelado);

    public static string Nombre(EstadoEnvio estado) => estado switch
    {
        EstadoEnvio.BuscandoConductor => "Buscando conductor",
        EstadoEnvio.ConductorAsignado => "Conductor asignado",
        EstadoEnvio.Retirado => "Retirado",
        EstadoEnvio.EnCamino => "En camino",
        EstadoEnvio.Entregado => "Entregado",
        EstadoEnvio.Cancelado => "Cancelado",
        _ => estado.ToString()
    };
}
