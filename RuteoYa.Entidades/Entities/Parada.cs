using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Entities;

public partial class Parada
{
    public int Id { get; set; }
    public int ViajeId { get; set; }
    public Viaje? viaje { get; set; }
    public int Orden { get; set; }
    public string Direccion { get; set; }
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}
