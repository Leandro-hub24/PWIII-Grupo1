using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;
using Xunit;

namespace RuteoYa.Tests;

public class OptimizadorServiceTests
{
    private readonly OptimizadorService _optimizador;

    public OptimizadorServiceTests()
    {
        _optimizador = new OptimizadorService(null!, null!);
    }

    // =========================================================================
    // TEST 1: El caso oficial del enunciado (Página 17 del PDF)
    // =========================================================================
    [Fact]
    public void CalcularMejorCombinacion_CasoOficialEnunciado_DebeElegirCombinacionGanadora()
    {
        var restricciones = new RestriccionesViajeDto
        {
            ViajeId = 1,
            MaxPaquetes = 3,
            PesoMaxKg = 10m,
            DesvioMaxKm = 5m
        };

        var candidatos = new List<OportunidadDto>
        {
            new() { EnvioId = 1, Codigo = "A", Pago = 4000m, PesoKg = 2m, DesvioKm = 1.0m },
            new() { EnvioId = 2, Codigo = "B", Pago = 5500m, PesoKg = 6m, DesvioKm = 2.0m },
            new() { EnvioId = 3, Codigo = "C", Pago = 3500m, PesoKg = 4m, DesvioKm = 1.0m },
            new() { EnvioId = 4, Codigo = "D", Pago = 7000m, PesoKg = 8m, DesvioKm = 3.0m },
            new() { EnvioId = 5, Codigo = "E", Pago = 2500m, PesoKg = 1m, DesvioKm = 0.5m }
        };

        var resultado = _optimizador.CalcularMejorCombinacion(restricciones, candidatos);

        Assert.NotNull(resultado.CombinacionRecomendada);
        Assert.Equal("A + B + E", resultado.CombinacionRecomendada.NombresCombinados);
        Assert.Equal(12000m, resultado.CombinacionRecomendada.GananciaTotal);
        Assert.Equal(9m, resultado.CombinacionRecomendada.PesoTotalKg);
        Assert.Equal(3.5m, resultado.CombinacionRecomendada.DesvioTotalKm);
    }

    // =========================================================================
    // TEST 2: Regla de Desempate (Si ganan lo mismo, gana el de MENOR desvío)
    // =========================================================================
    [Fact]
    public void CalcularMejorCombinacion_EmpateDeGanancia_DebeElegirMenorDesvio()
    {
        var restricciones = new RestriccionesViajeDto
        {
            MaxPaquetes = 1,
            PesoMaxKg = 10m,
            DesvioMaxKm = 10m
        };

        var candidatos = new List<OportunidadDto>
        {
            new() { EnvioId = 1, Codigo = "A", Pago = 5000m, PesoKg = 2m, DesvioKm = 3.0m },
            new() { EnvioId = 2, Codigo = "B", Pago = 5000m, PesoKg = 2m, DesvioKm = 1.5m }
        };

        var resultado = _optimizador.CalcularMejorCombinacion(restricciones, candidatos);

        Assert.NotNull(resultado.CombinacionRecomendada);
        Assert.Equal("B", resultado.CombinacionRecomendada.NombresCombinados);
        Assert.Equal(1.5m, resultado.CombinacionRecomendada.DesvioTotalKm);
    }

    // =========================================================================
    // TEST 3: Respeto de Peso Máximo
    // =========================================================================
    [Fact]
    public void CalcularMejorCombinacion_SuperaPesoMaximo_DebeDescartarYRegistrarMotivo()
    {
        var restricciones = new RestriccionesViajeDto
        {
            MaxPaquetes = 3,
            PesoMaxKg = 5m,
            DesvioMaxKm = 10m
        };

        var candidatos = new List<OportunidadDto>
        {
            new() { EnvioId = 1, Codigo = "Pesado", Pago = 20000m, PesoKg = 8m, DesvioKm = 1.0m },
            new() { EnvioId = 2, Codigo = "Liviano", Pago = 3000m, PesoKg = 2m, DesvioKm = 1.0m }
        };

        var resultado = _optimizador.CalcularMejorCombinacion(restricciones, candidatos);

        Assert.NotNull(resultado.CombinacionRecomendada);
        Assert.Equal("Liviano", resultado.CombinacionRecomendada.NombresCombinados);
        Assert.Contains(resultado.CombinacionesDescartadas, c => c.MotivoDescarte == "Supera 5 kg");
    }

    // =========================================================================
    // TEST 4: Lista vacía de candidatos
    // =========================================================================
    [Fact]
    public void CalcularMejorCombinacion_SinCandidatos_RetornaResultadoVacioSinErrores()
    {
        var restricciones = new RestriccionesViajeDto { ViajeId = 99, MaxPaquetes = 3, PesoMaxKg = 10m, DesvioMaxKm = 5m };
        var candidatosVacios = new List<OportunidadDto>();

        var resultado = _optimizador.CalcularMejorCombinacion(restricciones, candidatosVacios);

        Assert.Null(resultado.CombinacionRecomendada);
        Assert.Empty(resultado.OtrasCombinacionesValidas);
        Assert.Empty(resultado.CombinacionesDescartadas);
    }

    // =========================================================================
    // TEST 5: Capacidad de 4 paquetes
    // =========================================================================
    [Fact]
    public void CalcularMejorCombinacion_ConCapacidadDe4Paquetes_DebePermitirCombinar4()
    {
        var restricciones = new RestriccionesViajeDto
        {
            ViajeId = 1,
            MaxPaquetes = 4,
            PesoMaxKg = 20m,
            DesvioMaxKm = 10m
        };

        var candidatos = new List<OportunidadDto>
        {
            new() { EnvioId = 1, Codigo = "A", Pago = 1000m, PesoKg = 1m, DesvioKm = 0.5m },
            new() { EnvioId = 2, Codigo = "B", Pago = 1000m, PesoKg = 1m, DesvioKm = 0.5m },
            new() { EnvioId = 3, Codigo = "C", Pago = 1000m, PesoKg = 1m, DesvioKm = 0.5m },
            new() { EnvioId = 4, Codigo = "D", Pago = 1000m, PesoKg = 1m, DesvioKm = 0.5m },
            new() { EnvioId = 5, Codigo = "E", Pago = 100m, PesoKg = 1m, DesvioKm = 0.5m }
        };

        var resultado = _optimizador.CalcularMejorCombinacion(restricciones, candidatos);

        Assert.NotNull(resultado.CombinacionRecomendada);
        Assert.Equal(4, resultado.CombinacionRecomendada.CantidadPaquetes);
        Assert.Equal("A + B + C + D", resultado.CombinacionRecomendada.NombresCombinados);
        Assert.Equal(4000m, resultado.CombinacionRecomendada.GananciaTotal);
    }

    // =========================================================================
    // TEST 6: Respeto de Desvío Máximo
    // =========================================================================
    [Fact]
    public void CalcularMejorCombinacion_SuperaDesvioMaximo_DebeDescartarYRegistrarMotivo()
    {
        var restricciones = new RestriccionesViajeDto
        {
            MaxPaquetes = 3,
            PesoMaxKg = 20m,
            DesvioMaxKm = 2m
        };

        var candidatos = new List<OportunidadDto>
        {
            new() { EnvioId = 1, Codigo = "Lejos", Pago = 15000m, PesoKg = 1m, DesvioKm = 4.5m },
            new() { EnvioId = 2, Codigo = "Cerca", Pago = 3000m, PesoKg = 1m, DesvioKm = 1.0m }
        };

        var resultado = _optimizador.CalcularMejorCombinacion(restricciones, candidatos);

        Assert.NotNull(resultado.CombinacionRecomendada);
        Assert.Equal("Cerca", resultado.CombinacionRecomendada.NombresCombinados);
        Assert.Contains(resultado.CombinacionesDescartadas, c => c.MotivoDescarte == "Supera 2 km");
    }

    // =========================================================================
    // TEST 7: Seguridad - Si el viaje es de otro usuario, devuelve null (404)
    // =========================================================================
    [Fact]
    public async Task ObtenerOptimizacionPorViajeAsync_ViajeDeOtroUsuario_RetornaNull()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje { Id = 1, UsuarioId = 10, Activo = true, MaxPaquets = 3, PesoMaxKg = 10, DesvioMaxKm = 5 }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ObtenerOptimizacionPorViajeAsync(viajeId: 1, usuarioId: 99);

        Assert.Null(resultado);
    }

    // =========================================================================
    // TEST 8: Capacidad restante - Descuenta paquetes ya asignados al viaje
    // =========================================================================
    [Fact]
    public async Task ObtenerOptimizacionPorViajeAsync_ConPaquetesYaAsignados_DescuentaCapacidad()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenDir = "Belgrano",
                DestinoDir = "Lomas",
                Activo = true,
                MaxPaquets = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m
            },
            YaAsignados = new List<Envio>
            {
                new() { Id = 50, PesoKg = 4m, Asignacion = new Asignacion { Id = 1, DesvioKm = 1.0m } },
                new() { Id = 51, PesoKg = 2m, Asignacion = new Asignacion { Id = 2, DesvioKm = 0.5m } }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ObtenerOptimizacionPorViajeAsync(viajeId: 1, usuarioId: 1);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Restricciones.MaxPaquetes);     // 3 - 2 = 1
        Assert.Equal(4m, resultado.Restricciones.PesoMaxKg);      // 10 - 6 = 4
        Assert.Equal(3.5m, resultado.Restricciones.DesvioMaxKm);  // 5 - 1.5 = 3.5
    }

    // =========================================================================
    // TEST 9: Concurrencia (409) - Si otro conductor tomó un envío antes
    // =========================================================================
    [Fact]
    public async Task ConfirmarCombinacionAsync_EnvioYaTomadoPorOtro_Retorna409()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenDir = "A",
                DestinoDir = "B",
                Activo = true,
                MaxPaquets = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m
            },
            EnviosPorIds = new List<Envio>
            {
                new()
                {
                    Id = 5,
                    UsuarioId = 2,
                    Estado = (byte)EstadoEnvio.ConductorAsignado,
                    PesoKg = 2m,
                    Monto = 4000m,
                    FechaLimite = DateTime.Today.AddDays(1)
                }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ConfirmarCombinacionAsync(
            new ConfirmarAsignacionDto { ViajeId = 1, EnvioIds = new List<int> { 5 } },
            usuarioId: 1);

        Assert.Equal(TipoResultado.Conflicto, resultado.Tipo);
    }

    // =========================================================================
    // TEST 10: No debe incluir envíos propios del conductor al optimizar
    // =========================================================================
    [Fact]
    public async Task ObtenerOptimizacionPorViajeAsync_IgnoraEnviosPropiosDelConductor()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenDir = "Origen",
                OrigenLat = 0m,
                OrigenLng = 0m,
                DestinoDir = "Destino",
                DestinoLat = 0m,
                DestinoLng = 0.2m,
                Activo = true,
                MaxPaquets = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 10m
            },
            EnviosCompatibles = new List<Envio>
            {
                // Envío propio del conductor (UsuarioId = 1) -> debe ignorarse aunque pague más
                new()
                {
                    Id = 10, UsuarioId = 1, Estado = 1, PesoKg = 1m, Monto = 9999m,
                    OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                    FechaLimite = DateTime.Today.AddDays(1)
                },
                // Envío de otro usuario (UsuarioId = 2) -> debe incluirse
                new()
                {
                    Id = 11, UsuarioId = 2, Estado = 1, PesoKg = 2m, Monto = 4000m,
                    OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                    FechaLimite = DateTime.Today.AddDays(1)
                }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ObtenerOptimizacionPorViajeAsync(viajeId: 1, usuarioId: 1);

        Assert.NotNull(resultado?.CombinacionRecomendada);
        Assert.Single(resultado!.CombinacionRecomendada!.Envios);
        Assert.Equal(11, resultado.CombinacionRecomendada.Envios[0].EnvioId);
        Assert.Equal(4000m, resultado.CombinacionRecomendada.GananciaTotal);
    }

    // =========================================================================
    // TEST 11: No debe permitir confirmar un envío propio (400)
    // =========================================================================
    [Fact]
    public async Task ConfirmarCombinacionAsync_EnvioPropio_Retorna400()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                Activo = true,
                MaxPaquets = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m
            },
            EnviosPorIds = new List<Envio>
            {
                new()
                {
                    Id = 10, UsuarioId = 1, Estado = (byte)EstadoEnvio.BuscandoConductor,
                    PesoKg = 2m, Monto = 4000m, FechaLimite = DateTime.Today.AddDays(1)
                }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ConfirmarCombinacionAsync(
            new ConfirmarAsignacionDto { ViajeId = 1, EnvioIds = new List<int> { 10 } },
            usuarioId: 1);

        Assert.Equal(TipoResultado.Invalido, resultado.Tipo);
    }

    // =========================================================================
    // TEST 12: Respeta días de viaje y fecha límite del envío
    // =========================================================================
    [Fact]
    public async Task ObtenerOptimizacionPorViajeAsync_RespetaDiasYFechaLimite()
    {
        // El viaje solo circula dentro de 3 días (funciona sin importar qué día de la semana sea hoy)
        var diaDelViaje = (byte)DateTime.Today.AddDays(3).DayOfWeek;

        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenLat = 0m,
                OrigenLng = 0m,
                DestinoLat = 0m,
                DestinoLng = 0.2m,
                Activo = true,
                MaxPaquets = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 10m,
                ViajeDias = new List<ViajeDia> { new() { DiaSemana = diaDelViaje } }
            },
            EnviosCompatibles = new List<Envio>
            {
                // Vence mañana (antes del viaje) -> NO debe aparecer
                new()
                {
                    Id = 20, UsuarioId = 2, Estado = 1, PesoKg = 1m, Monto = 9000m,
                    OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                    FechaLimite = DateTime.Today.AddDays(1)
                },
                // Vence en 4 días (cubre el día del viaje) -> SÍ debe aparecer
                new()
                {
                    Id = 21, UsuarioId = 2, Estado = 1, PesoKg = 2m, Monto = 3500m,
                    OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                    FechaLimite = DateTime.Today.AddDays(4)
                }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ObtenerOptimizacionPorViajeAsync(viajeId: 1, usuarioId: 1);

        Assert.NotNull(resultado?.CombinacionRecomendada);
        Assert.Single(resultado!.CombinacionRecomendada!.Envios);
        Assert.Equal(21, resultado.CombinacionRecomendada.Envios[0].EnvioId);
    }

    // =========================================================================
    // TEST 13: Considera paradas intermedias del viaje al calcular el desvío
    // =========================================================================
    [Fact]
    public async Task ObtenerOptimizacionPorViajeAsync_ConParadasIntermedias_EvaluaEnvioEnTramos()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenLat = 0m,
                OrigenLng = 0m,
                DestinoLat = 0m,
                DestinoLng = 0.2m,
                Activo = true,
                MaxPaquets = 2,
                PesoMaxKg = 10m,
                DesvioMaxKm = 1m,
                Paradas = new List<Parada>
                {
                    new() { Id = 1, Orden = 1, Direccion = "Parada 1", Lat = 0m, Lng = 0.1m }
                }
            },
            EnviosCompatibles = new List<Envio>
            {
                // Retira en tramo 1 (0.05) y entrega en tramo 2 (0.15) pasando por la parada (0.1) sin desvío extra
                new()
                {
                    Id = 30, UsuarioId = 2, Estado = 1, PesoKg = 2m, Monto = 4500m,
                    OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                    FechaLimite = DateTime.Today.AddDays(1)
                }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ObtenerOptimizacionPorViajeAsync(viajeId: 1, usuarioId: 1);

        Assert.NotNull(resultado?.CombinacionRecomendada);
        Assert.Equal(30, resultado!.CombinacionRecomendada!.Envios[0].EnvioId);
        Assert.Equal(0m, resultado.CombinacionRecomendada.DesvioTotalKm);
    }

    // =========================================================================
    // TEST 14: Confirmación exitosa (201) guarda ganancia individual por envío
    // =========================================================================
    [Fact]
    public async Task ConfirmarCombinacionAsync_Valido_Retorna201YGuardaGananciaIndividual()
    {
        var fakeRepo = new FakeOportunidadesRepositorio
        {
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenLat = 0m,
                OrigenLng = 0m,
                DestinoLat = 0m,
                DestinoLng = 0.2m,
                Activo = true,
                MaxPaquets = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m
            },
            EnviosPorIds = new List<Envio>
            {
                new()
                {
                    Id = 1, UsuarioId = 2, Estado = (byte)EstadoEnvio.BuscandoConductor,
                    PesoKg = 3m, Monto = 4000m,
                    OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.10m,
                    FechaLimite = DateTime.Today.AddDays(2)
                },
                new()
                {
                    Id = 2, UsuarioId = 3, Estado = (byte)EstadoEnvio.BuscandoConductor,
                    PesoKg = 6m, Monto = 5500m,
                    OrigenLat = 0m, OrigenLng = 0.10m, DestinoLat = 0m, DestinoLng = 0.15m,
                    FechaLimite = DateTime.Today.AddDays(2)
                }
            }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.ConfirmarCombinacionAsync(
            new ConfirmarAsignacionDto { ViajeId = 1, EnvioIds = new List<int> { 1, 2 } },
            usuarioId: 1);

        Assert.Equal(TipoResultado.Ok, resultado.Tipo);
        Assert.Equal(2, fakeRepo.UltimosEnviosAsignados.Count);
        Assert.Equal(4000m, fakeRepo.UltimosEnviosAsignados.Single(x => x.EnvioId == 1).Ganancia);
        Assert.Equal(5500m, fakeRepo.UltimosEnviosAsignados.Single(x => x.EnvioId == 2).Ganancia);
    }

    // =========================================================================
    // TEST 15: Potencial Mensual - No duplica un mismo envío entre viajes del mes
    // =========================================================================
    [Fact]
    public async Task CalcularPotencialMensualAsync_NoDuplicaEnvioEntreViajesDelMes()
    {
        var fechaFutura = DateTime.Today.AddDays(1);
        // Nos aseguramos de probar dentro del mes de fechaFutura
        var diaSemana = (byte)fechaFutura.DayOfWeek;

        var fakeRepo = new FakeOportunidadesRepositorio
        {
            GananciaEfectivaMes = 3000m,
            Viaje = new Viaje
            {
                Id = 1,
                UsuarioId = 1,
                OrigenDir = "Belgrano",
                OrigenLat = 0m,
                OrigenLng = 0m,
                DestinoDir = "Lomas",
                DestinoLat = 0m,
                DestinoLng = 0.2m,
                Activo = true,
                MaxPaquets = 2,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m,
                ViajeDias = new List<ViajeDia> { new() { DiaSemana = diaSemana } }
            },
            EnviosCompatibles = new List<Envio>
        {
            new()
            {
                Id = 99, UsuarioId = 2, Estado = 1, PesoKg = 2m, Monto = 5000m,
                OrigenDir = "A", OrigenLat = 0m, OrigenLng = 0.05m,
                DestinoDir = "B", DestinoLat = 0m, DestinoLng = 0.15m,
                FechaLimite = new DateTime(fechaFutura.Year, fechaFutura.Month,
                    DateTime.DaysInMonth(fechaFutura.Year, fechaFutura.Month))
            }
        }
        };
        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());

        var resultado = await service.CalcularPotencialMensualAsync(
            usuarioId: 1, anio: fechaFutura.Year, mes: fechaFutura.Month, criterio: "ganancias");

        Assert.Equal(TipoResultado.Ok, resultado.Tipo);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(3000m, resultado.Valor!.GananciaEfectivaObtenida);
        // Aunque el viaje ocurra varias veces en el mes, el envío #99 solo se cuenta 1 vez
        Assert.Equal(5000m, resultado.Valor.GananciaPotencialTotal);
        Assert.Equal(1, resultado.Valor.PaquetesPosibles);
    }

    // =========================================================================
    // TEST 16: Potencial Mensual - Criterio inválido devuelve Invalido (400)
    // =========================================================================
    [Fact]
    public async Task CalcularPotencialMensualAsync_CriterioInvalido_RetornaInvalido()
    {
        var service = new OptimizadorService(new FakeOportunidadesRepositorio(), new FakeDistanciaService());

        var resultado = await service.CalcularPotencialMensualAsync(1, DateTime.Today.Year, DateTime.Today.Month, "cualquiera");

        Assert.Equal(TipoResultado.Invalido, resultado.Tipo);
    }

    // =========================================================================
    // TEST 17: Auditoría completa de métricas mensuales (semanas, promedio, mejor recorrido)
    // =========================================================================
    [Fact]
    public async Task CalcularPotencialMensualAsync_DosViajes_CalculaMetricasExactas()
    {
        var mesSiguiente = DateTime.Today.AddMonths(1);
        int anio = mesSiguiente.Year;
        int mes = mesSiguiente.Month;
        var primerDia = new DateTime(anio, mes, 1);

        // Viaje 1 circula el día 1 (y 8, 15, 22...); Viaje 2 circula el día 2 (y 9, 16, 23...)
        var dia1 = (byte)primerDia.DayOfWeek;
        var dia2 = (byte)primerDia.AddDays(1).DayOfWeek;

        int cantDia1 = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
            .Count(d => (byte)new DateTime(anio, mes, d).DayOfWeek == dia1);
        int cantDia2 = Enumerable.Range(1, DateTime.DaysInMonth(anio, mes))
            .Count(d => (byte)new DateTime(anio, mes, d).DayOfWeek == dia2);
        int totalOcurrencias = cantDia1 + cantDia2;

        var fakeRepo = new FakeOportunidadesRepositorio
        {
            GananciaEfectivaMes = 12000m,
            ViajesActivos = new List<Viaje>
        {
            new()
            {
                Id = 1, UsuarioId = 1, OrigenDir = "Belgrano", DestinoDir = "Lomas",
                OrigenLat = 0m, OrigenLng = 0m, DestinoLat = 0m, DestinoLng = 0.2m,
                Activo = true, MaxPaquets = 2, PesoMaxKg = 10m, DesvioMaxKm = 1m,
                ViajeDias = new List<ViajeDia> { new() { DiaSemana = dia1 } }
            },
            new()
            {
                Id = 2, UsuarioId = 1, OrigenDir = "Palermo", DestinoDir = "Quilmes",
                OrigenLat = 0m, OrigenLng = 0m, DestinoLat = 0m, DestinoLng = 0.2m,
                Activo = true, MaxPaquets = 2, PesoMaxKg = 10m, DesvioMaxKm = 5m,
                ViajeDias = new List<ViajeDia> { new() { DiaSemana = dia2 } }
            }
        },
            EnviosCompatibles = new List<Envio>
        {
            // Desvío 0 km -> entra en Viaje 1 (día 1, Semana 1) por $4.000
            new()
            {
                Id = 101, UsuarioId = 2, Estado = 1, PesoKg = 2m, Monto = 4000m,
                OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                FechaLimite = new DateTime(anio, mes, 15)
            },
            // Desvío 3.61 km -> NO entra en Viaje 1 (DesvioMax = 1), entra en Viaje 2 (día 2, Semana 1) por $5.500
            new()
            {
                Id = 102, UsuarioId = 2, Estado = 1, PesoKg = 2m, Monto = 5500m,
                OrigenLat = 0.05m, OrigenLng = 0.10m, DestinoLat = 0.05m, DestinoLng = 0.15m,
                FechaLimite = new DateTime(anio, mes, 15)
            }
        }
        };

        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());
        var res = await service.CalcularPotencialMensualAsync(1, anio, mes, "ganancias");

        Assert.Equal(TipoResultado.Ok, res.Tipo);
        var dto = res.Valor!;
        Assert.Equal(9500m, dto.GananciaPotencialTotal);
        Assert.Equal(12000m, dto.GananciaEfectivaObtenida);
        Assert.Equal(totalOcurrencias, dto.ViajesAnalizados);
        Assert.Equal(2, dto.PaquetesPosibles);
        Assert.Equal(Math.Round(9500m / totalOcurrencias, 2), dto.PromedioPorViaje);
        Assert.Equal("Palermo → Quilmes", dto.MejorRecorrido);
        Assert.Equal(5500m, dto.MejorRecorridoGanancia);

        // Ambos envíos se asignaron en Semana 1 (días 1 y 2) y no se repitieron en semanas siguientes
        var semana1 = dto.GananciaPorSemana.Single(s => s.Semana == 1);
        Assert.Equal(9500m, semana1.GananciaPotencial);
        Assert.Equal(2, semana1.Paquetes);
    }

    // =========================================================================
    // TEST 18: Verifica que los 3 criterios (ganancias, desvios, equilibrio) funcionan distinto
    // =========================================================================
    [Theory]
    [InlineData("ganancias", 3, 7000)]   // El que más paga ($7.000 aunque desvíe 3.61 km)
    [InlineData("desvios", 1, 1000)]     // El que menos desvía (0 km aunque pague $1.000)
    [InlineData("equilibrio", 2, 6000)]  // Mejor relación ganancia/km ($6.000 con solo 0.17 km)
    public async Task CalcularPotencialMensualAsync_Criterios_EligenSegunEstrategia(
        string criterio, int envioEsperadoId, decimal gananciaEsperada)
    {
        var mesSiguiente = DateTime.Today.AddMonths(1);
        int anio = mesSiguiente.Year;
        int mes = mesSiguiente.Month;
        var dia1 = (byte)new DateTime(anio, mes, 1).DayOfWeek;

        var fakeRepo = new FakeOportunidadesRepositorio
        {
            ViajesActivos = new List<Viaje>
        {
            new()
            {
                Id = 1, UsuarioId = 1, OrigenDir = "A", DestinoDir = "B",
                OrigenLat = 0m, OrigenLng = 0m, DestinoLat = 0m, DestinoLng = 0.2m,
                Activo = true, MaxPaquets = 1, PesoMaxKg = 10m, DesvioMaxKm = 10m,
                ViajeDias = new List<ViajeDia> { new() { DiaSemana = dia1 } }
            }
        },
            EnviosCompatibles = new List<Envio>
        {
            // Envío 1: $1.000, desvío 0 km
            new()
            {
                Id = 1, UsuarioId = 2, Estado = 1, PesoKg = 1m, Monto = 1000m,
                OrigenLat = 0m, OrigenLng = 0.05m, DestinoLat = 0m, DestinoLng = 0.15m,
                FechaLimite = new DateTime(anio, mes, 2)
            },
            // Envío 2: $6.000, desvío ~0.17 km
            new()
            {
                Id = 2, UsuarioId = 2, Estado = 1, PesoKg = 1m, Monto = 6000m,
                OrigenLat = 0.01m, OrigenLng = 0.10m, DestinoLat = 0m, DestinoLng = 0.15m,
                FechaLimite = new DateTime(anio, mes, 2)
            },
            // Envío 3: $7.000, desvío ~3.61 km
            new()
            {
                Id = 3, UsuarioId = 2, Estado = 1, PesoKg = 1m, Monto = 7000m,
                OrigenLat = 0.05m, OrigenLng = 0.10m, DestinoLat = 0.05m, DestinoLng = 0.15m,
                FechaLimite = new DateTime(anio, mes, 2)
            }
        }
        };

        var service = new OptimizadorService(fakeRepo, new FakeDistanciaService());
        var res = await service.CalcularPotencialMensualAsync(1, anio, mes, criterio);

        Assert.Equal(TipoResultado.Ok, res.Tipo);
        Assert.Equal(gananciaEsperada, res.Valor!.GananciaPotencialTotal);
        var primerViajeConPaquete = res.Valor.DetalleViajes.First(d => d.PaquetesSugeridos.Any());
        Assert.Equal(envioEsperadoId, primerViajeConPaquete.PaquetesSugeridos[0].EnvioId);
    }

    // =========================================================================
    // FAKES SIMPLES EN MEMORIA PARA LOS TESTS
    // =========================================================================
    private class FakeDistanciaService : IDistanciaService
    {


        public Task<DistanciaRutaDto> CalcularAsync(string origenDireccion, string destinoDireccion)
            => Task.FromResult(new DistanciaRutaDto { DistanciaKm = 12.5, DuracionMinutos = 25 });

        public Task<DistanciaRutaDto> CalcularPorCoordenadasAsync(double origenLat, double origenLng, double destinoLat, double destinoLng)
        {
            double diffLat = destinoLat - origenLat;
            double diffLng = destinoLng - origenLng;
            double distanciaKm = Math.Round(Math.Sqrt(diffLat * diffLat + diffLng * diffLng) * 111.0, 2);

            return Task.FromResult(new DistanciaRutaDto
            {
                DistanciaKm = distanciaKm,
                DuracionMinutos = (int)Math.Round(distanciaKm * 2)
            });
        }
    }

    private class FakeOportunidadesRepositorio : IOportunidadesRepositorio
    {
        public Viaje? Viaje { get; set; }
        public List<Envio> EnviosCompatibles { get; set; } = new();
        public List<Envio> EnviosPorIds { get; set; } = new();
        public List<Envio> YaAsignados { get; set; } = new();
        public List<AsignacionEnvioDetalleDto> UltimosEnviosAsignados { get; private set; } = new();

        public Task<Viaje?> GetViajePorIdAsync(int id) => Task.FromResult(Viaje);
        public Task<List<Envio>> GetEnviosCompatiblesAsync(decimal pesoMaximo) => Task.FromResult(EnviosCompatibles);
        public Task<Envio?> GetEnvioPorIdAsync(int id) => Task.FromResult<Envio?>(null);
        public Task<(int Paquetes, decimal PesoKg)> GetCargaAsignadaAsync(int viajeId) => Task.FromResult((0, 0m));
        public Task<bool> AsignarEnvioAsync(int envioId, int viajeId, byte nuevoEstado) => Task.FromResult(true);
        public Task<List<Envio>> GetEnviosPorIdsAsync(List<int> ids) => Task.FromResult(EnviosPorIds);
        public Task<List<Envio>> GetEnviosAsignadosAViajeAsync(int viajeId) => Task.FromResult(YaAsignados);
        //public Task<List<Viaje>> GetViajesActivosDeUsuarioAsync(int usuarioId)
        //    => Task.FromResult(Viaje != null ? new List<Viaje> { Viaje } : new List<Viaje>());

        public Task<(bool Exito, string? Mensaje)> AsignarEnviosViajesAsync(
            int viajeId, List<AsignacionEnvioDetalleDto> envios, DateTime fechaViaje)
        {
            UltimosEnviosAsignados = envios;
            return Task.FromResult<(bool, string?)>((true, null));
        }

        public decimal GananciaEfectivaMes { get; set; }

        public Task<decimal> GetGananciaEfectivaMesAsync(int conductorId, int anio, int mes)
            => Task.FromResult(GananciaEfectivaMes);

        // Para ganancias potenciales
        public List<Viaje> ViajesActivos { get; set; } = new();

        public Task<List<Viaje>> GetViajesActivosDeUsuarioAsync(int usuarioId)
        {
            if (ViajesActivos.Any()) return Task.FromResult(ViajesActivos);
            return Task.FromResult(Viaje != null ? new List<Viaje> { Viaje } : new List<Viaje>());
        }
    }

}