namespace RuteoYa.Entidades;

public partial class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreadoEn { get; set; }

    public int ViajeId { get; set; } 
    public List<Viaje> Viajes { get; set; } = new List<Viaje>();
    public int EnvioId { get; set; }
    public List<Envio> Envios { get; set; } = new List<Envio>();
}
