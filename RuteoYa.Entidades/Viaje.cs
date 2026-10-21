using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades;

public partial class Viaje
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string OrigenDir { get; set; }
    public decimal OrigenLat { get; set; }
    public decimal OrigenLng { get; set; }
    public decimal DestinoLat { get; set; }
    public decimal DestinoLng { get; set; }
    public TimeOnly HoraSalida { get; set; }
    public TimeOnly HoraLlegada { get; set; }
    public int MaxPaquets { get; set; }
    public decimal PesoMaxKg { get; set; }
    public decimal DesvioMaxKm { get; set; }
    public int DistanciaMin { get; set; }
    public bool Activo { get; set; }

    public int AsignacionId { get; set; }
    public List<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    public int ViajeDiaId { get; set; }
    public List<ViajeDia> ViajeDias { get; set; } = new List<ViajeDia>();
    public int ParadaId { get; set; }
    public List<Parada> Paradas { get; set; } = new List<Parada>();
}
