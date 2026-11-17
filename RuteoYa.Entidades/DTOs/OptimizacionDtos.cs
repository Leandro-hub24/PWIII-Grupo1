using System.Collections.Generic;
using System.Linq;

namespace RuteoYa.Entidades.DTOs
{
    public class OportunidadDto
    {
        public int EnvioId { get; set; }
        public string Codigo { get; set; } = string.Empty; 
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public decimal Pago { get; set; } 
        public decimal PesoKg { get; set; }
        public decimal DesvioKm { get; set; }
    }

    // Límites del viaje del conductor
    public class RestriccionesViajeDto
    {
        public int ViajeId { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public int MaxPaquetes { get; set; }
        public decimal PesoMaxKg { get; set; }
        public decimal DesvioMaxKm { get; set; }
    }

    // Cada combo evaluado 
    public class CombinacionDto
    {
        public List<OportunidadDto> Envios { get; set; } = new List<OportunidadDto>();
        public decimal GananciaTotal { get; set; }
        public decimal PesoTotalKg { get; set; }
        public decimal DesvioTotalKm { get; set; }
        public int CantidadPaquetes => Envios.Count;
        public bool EsValida { get; set; }
        public string? MotivoDescarte { get; set; }
        public string NombresCombinados => string.Join(" + ", Envios.Select(e => e.Codigo));
    }

    // Respuesta completa que devuelve la API en GET /api/optimizacion/{viajeId}
    public class ResultadoOptimizacionDto
    {
        public int ViajeId { get; set; }
        public RestriccionesViajeDto Restricciones { get; set; } = new RestriccionesViajeDto();
        public CombinacionDto? CombinacionRecomendada { get; set; }
        public List<CombinacionDto> OtrasCombinacionesValidas { get; set; } = new List<CombinacionDto>();
        public List<CombinacionDto> CombinacionesDescartadas { get; set; } = new List<CombinacionDto>();
    }

    // Para confirmar la reserva de varios envios juntos
    public class ConfirmarAsignacionDto
    {
        public int ViajeId { get; set; }
        public List<int> EnvioIds { get; set; } = new List<int>();
    }

    public class ViajeResumenDto
    {
        public int Id { get; set; }
        public string OrigenDir { get; set; } = string.Empty;
        public string DestinoDir { get; set; } = string.Empty;
        public string Descripcion => $"{OrigenDir} → {DestinoDir}";
    }

    public class AsignacionEnvioDetalleDto
    {
        public int EnvioId { get; set; }
        public decimal DesvioKm { get; set; }
        public int TiempoExtraMin { get; set; }
        public decimal Ganancia { get; set; }
    }

    // DTOs para ganancia potencial

    public class PotencialMensualDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Criterio { get; set; } = "ganancias";
        public decimal GananciaPotencialTotal { get; set; }
        public decimal GananciaEfectivaObtenida { get; set; }
        public int ViajesAnalizados { get; set; }
        public int PaquetesPosibles { get; set; }
        public decimal PromedioPorViaje { get; set; }
        public string? MejorRecorrido { get; set; }
        public decimal MejorRecorridoGanancia { get; set; }
        public List<GananciaSemanalDto> GananciaPorSemana { get; set; } = new();
        public List<ViajePotencialDetalleDto> DetalleViajes { get; set; } = new();
    }

    public class GananciaSemanalDto
    {
        public int Semana { get; set; }
        public decimal GananciaPotencial { get; set; }
        public int Viajes { get; set; }
        public int Paquetes { get; set; }
    }

    public class ViajePotencialDetalleDto
    {
        public int ViajeId { get; set; }
        public string Recorrido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal GananciaPotencial { get; set; }
        public decimal DesvioTotalKm { get; set; }
        public List<OportunidadDto> PaquetesSugeridos { get; set; } = new();
    }
}