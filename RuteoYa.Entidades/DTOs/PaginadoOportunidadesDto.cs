namespace RuteoYa.Entidades.DTOs;

public class PaginadoOportunidadesDto<T>
{
    public List<T> Items { get; set; } = new(); //Lsta de envios correspondientes a la paguina visible.
    public int PaginaActual { get; set; } = 1;
    public int TamanioDePagina { get; set; } = 10;
    public int TotalDeItems { get; set; }

    public int TotalDePaginas => TamanioDePagina > 0 ? (int)Math.Ceiling(TotalDeItems / (double)TamanioDePagina) : 0;
}
