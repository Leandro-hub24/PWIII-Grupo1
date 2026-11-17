namespace RuteoYa.Entidades.Common;

/// <summary>
/// Resultado de una operación de negocio, para que el controller elija el código HTTP
/// (Ok = 200/201, NoEncontrado = 404, Invalido = 400, Conflicto = 409).
/// </summary>
public enum TipoResultado { Ok, NoEncontrado, Invalido, Conflicto }

public class ResultadoOperacion<T>
{
    public TipoResultado Tipo { get; init; }
    public string? Mensaje { get; init; }
    public T? Valor { get; init; }

    public static ResultadoOperacion<T> Exito(T valor) => new() { Tipo = TipoResultado.Ok, Valor = valor };
    public static ResultadoOperacion<T> NoEncontrado(string msg) => new() { Tipo = TipoResultado.NoEncontrado, Mensaje = msg };
    public static ResultadoOperacion<T> Invalido(string msg) => new() { Tipo = TipoResultado.Invalido, Mensaje = msg };
    public static ResultadoOperacion<T> Conflicto(string msg) => new() { Tipo = TipoResultado.Conflicto, Mensaje = msg };
}
