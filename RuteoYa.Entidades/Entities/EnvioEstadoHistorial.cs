using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.Entities;

public partial class EnvioEstadoHistorial
{
    public int Id { get; set; }
    public int EnvioId { get; set; }
    public Envio? Envio { get; set; }
    public byte EstadoAnterior { get; set; }
    public byte EstadoNuevo { get; set; }
    public DateTime Fecha { get; set; }
}
