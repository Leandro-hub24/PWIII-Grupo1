using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Entities;

public partial class Asignacion
{
    public int Id { get; set; }
    public int ViajeId { get; set; }
    public Viaje? Viaje { get; set; }
    public DateTime FechaViaje { get; set; }
    public decimal DesvioKm { get; set; }
    public int TiempoExtraMin { get; set; }
    public decimal Ganancia { get; set; }
    public DateTime AsignadoEn { get; set; }
}
