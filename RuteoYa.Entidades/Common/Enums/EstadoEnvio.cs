namespace RuteoYa.Entidades.Common.Enums;

/// <summary>
/// Estados de un envío (se guardan como byte en Envio.Estado).
/// </summary>
public enum EstadoEnvio : byte
{
    BuscandoConductor = 1,
    ConductorAsignado = 2,
    Retirado = 3,
    EnCamino = 4,
    Entregado = 5,
    Cancelado = 6
}

/// <summary>
/// Tamaño del paquete (se guarda como byte en Envio.Tamano).
/// </summary>
public enum TamanoEnvio : byte
{
    Pequeno = 1,
    Mediano = 2,
    Grande = 3
}
