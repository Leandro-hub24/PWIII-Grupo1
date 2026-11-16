namespace RuteoYa.Entidades.Entities;

public partial class Envio
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string OrigenDir { get; set; }
    public decimal OrigenLat { get; set; }
    public decimal OrigenLng { get; set; }
    public string DestinoDir { get; set; }
    public decimal DestinoLat { get; set; }
    public decimal DestinoLng { get; set; }
    public DateTime FechaLimite { get; set; }
    public decimal PesoKg { get; set; }
    public byte Tamano { get; set; }
    public string Descripcion { get; set; }
    public decimal Monto { get; set; }
    public byte Estado { get; set; }
    public DateTime CreadoEn { get; set; }
    public byte RowVersion { get; set; }
    // Nullable: un envío recién publicado todavía no tiene conductor asignado.
    public int? AsignacionId { get; set; }
    public Asignacion? Asignacion { get; set; }
    public List<EnvioEstadoHistorial> EnvioEstadoHistorial { get; set; } = new List<EnvioEstadoHistorial>();

}
