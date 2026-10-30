using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Entities;

public partial class ViajeDia
{
    public int Id { get; set; }
    public int ViajeId { get; set; }
    public Viaje? Viaje { get; set; }
    public byte DiaSemana { get; set; }
}
