using System;
using System.Collections.Generic;
using System.Text;

namespace RuteoYa.Entidades.DTOs
{
    public class DetalleOportunidadDto
    {
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public decimal DistanciaHabitualKm { get; set; }
        public decimal DistanciaConEnvioKm { get; set; }
        public decimal DesvioKm { get; set; }
        public int TiempoHabitualMin { get; set; }
        public int TiempoAdicionalMin { get; set; }
        public decimal Pago { get; set; }
        public decimal PesoKg { get; set; }
        public decimal CapacidadRestanteKg { get; set; }
        public int PaquetesRestantes { get; set; }
        public decimal GananciaPorKmAdicional { get; set; }
    }
}
