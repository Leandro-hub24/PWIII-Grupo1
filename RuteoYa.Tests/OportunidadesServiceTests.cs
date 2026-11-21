using Moq;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;

namespace RuteoYa.Tests;

public class OportunidadesServiceTests
{
    private readonly Mock<IOportunidadesRepositorio> _repoMock = new();
    private readonly Mock<IDistanciaService> _distanciaServiceMock = new();
    private readonly OportunidadesService _service;

    public OportunidadesServiceTests()
    {
        _service = new OportunidadesService(_repoMock.Object, _distanciaServiceMock.Object);
    }

    private static Viaje ViajeEjemplo(int id = 1, int usuarioId = 10) => new()
    {
        Id = id,
        UsuarioId = usuarioId,
        OrigenLat = -34.6m,
        OrigenLng = -58.5m,
        DestinoLat = -34.7m,
        DestinoLng = -58.6m,
        PesoMaxKg = 50m,
        MaxPaquets = 5,
        DesvioMaxKm = 10m,
        Activo = true
    };

    private static Envio EnvioEjemplo(int id = 100, int usuarioId = 20, decimal peso = 5m, decimal monto = 2500m) => new()
    {
        Id = id,
        UsuarioId = usuarioId,
        OrigenLat = -34.61m,
        OrigenLng = -58.51m,
        DestinoLat = -34.65m,
        DestinoLng = -58.55m,
        PesoKg = peso,
        Monto = monto,
        Estado = (byte)EstadoEnvio.BuscandoConductor,
        OrigenDir = "Origen Test",
        DestinoDir = "Destino Test"
    };

    // ---------- ObtenerViajePorIdAsync ----------

    [Fact]
    public async Task ObtenerViajePorId_DevuelveViajeSiExiste()
    {
        var viaje = ViajeEjemplo();
        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);

        var resultado = await _service.ObtenerViajePorIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
    }

    // ---------- ObtenerEnviosCompatiblesAsync ----------

    [Fact]
    public async Task ObtenerEnviosCompatibles_ViajeInexistente_DevuelveListaVacia()
    {
        _repoMock.Setup(x => x.GetViajePorIdAsync(99)).ReturnsAsync((Viaje?)null);

        var resultado = await _service.ObtenerEnviosCompatiblesAsync(99, null);

        Assert.Empty(resultado.Items);
    }

    [Fact]
    public async Task ObtenerEnviosCompatibles_SuperaCapacidadPaquetes_DevuelveListaVacia()
    {
        var viaje = ViajeEjemplo();
        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);
        _repoMock.Setup(x => x.GetCargaAsignadaAsync(1)).ReturnsAsync((5, 10m));

        var resultado = await _service.ObtenerEnviosCompatiblesAsync(1, null);

        Assert.Empty(resultado.Items);
    }

    [Fact]
    public async Task ObtenerEnviosCompatibles_FiltraYOrdenaCorrectamente()
    {
        var viaje = ViajeEjemplo(usuarioId: 10);
        var envioValido = EnvioEjemplo(id: 101, usuarioId: 20);

        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);
        _repoMock.Setup(x => x.GetCargaAsignadaAsync(1)).ReturnsAsync((1, 10m));
        _repoMock.Setup(x => x.GetEnviosCompatiblesAsync(It.IsAny<decimal>()))
             .ReturnsAsync(new List<Envio> { envioValido });

        // Configuramos cualquier llamada al servicio de distancia para retornar distancias válidas
        _distanciaServiceMock.Setup(x => x.CalcularPorCoordenadasAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
             .ReturnsAsync(new DistanciaRutaDto { DistanciaKm = 3.33, DuracionMinutos = 5 });

        var resultado = await _service.ObtenerEnviosCompatiblesAsync(1, "desvio");

        Assert.Single(resultado.Items);
        Assert.Equal(101, resultado.Items[0].Id);
    }

    // ---------- CalcularDetalleOportunidadAsync ----------

    [Fact]
    public async Task CalcularDetalleOportunidad_SiViajeOEnvioNoExisten_DevuelveNull()
    {
        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync((Viaje?)null);
        _repoMock.Setup(x => x.GetEnvioPorIdAsync(100)).ReturnsAsync(EnvioEjemplo());

        var resultado = await _service.CalcularDetalleOportunidadAsync(1, 100);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task CalcularDetalleOportunidad_ConDatosValidos_DevuelveDetalleDto()
    {
        var viaje = ViajeEjemplo();
        var envio = EnvioEjemplo();

        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);
        _repoMock.Setup(x => x.GetEnvioPorIdAsync(100)).ReturnsAsync(envio);
        _repoMock.Setup(x => x.GetCargaAsignadaAsync(1)).ReturnsAsync((1, 10m));

        _distanciaServiceMock.Setup(x => x.CalcularPorCoordenadasAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
             .ReturnsAsync(new DistanciaRutaDto { DistanciaKm = 3.33, DuracionMinutos = 5 });

        var resultado = await _service.CalcularDetalleOportunidadAsync(1, 100);

        Assert.NotNull(resultado);
        Assert.Equal(100, resultado.EnvioId);
        Assert.Equal(1, resultado.ViajeId);
        Assert.Equal(envio.Monto, resultado.Pago);
    }

    // ---------- AceptarOportunidadAsync ----------

    [Fact]
    public async Task AceptarOportunidad_MismoUsuario_DevuelveError()
    {
        var viaje = ViajeEjemplo(usuarioId: 10);
        var envio = EnvioEjemplo(usuarioId: 10);

        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);
        _repoMock.Setup(x => x.GetEnvioPorIdAsync(100)).ReturnsAsync(envio);

        var resultado = await _service.AceptarOportunidadAsync(100, 1);

        Assert.False(resultado.Exito);
        Assert.Equal("No podés aceptar tu propio envío.", resultado.Mensaje);
    }

    [Fact]
    public async Task AceptarOportunidad_SuperaPesoMaximo_DevuelveError()
    {
        var viaje = ViajeEjemplo(usuarioId: 10);
        viaje.PesoMaxKg = 10m;

        var envio = EnvioEjemplo(usuarioId: 20, peso: 8m);

        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);
        _repoMock.Setup(x => x.GetEnvioPorIdAsync(100)).ReturnsAsync(envio);
        _repoMock.Setup(x => x.GetCargaAsignadaAsync(1)).ReturnsAsync((1, 5m));

        var resultado = await _service.AceptarOportunidadAsync(100, 1);

        Assert.False(resultado.Exito);
        Assert.Equal("El envío supera la capacidad de peso restante del viaje.", resultado.Mensaje);
    }

    [Fact]
    public async Task AceptarOportunidad_CondicionesValidas_AsignaExitosamente()
    {
        var viaje = ViajeEjemplo(usuarioId: 10);
        var envio = EnvioEjemplo(usuarioId: 20, peso: 5m);

        _repoMock.Setup(x => x.GetViajePorIdAsync(1)).ReturnsAsync(viaje);
        _repoMock.Setup(x => x.GetEnvioPorIdAsync(100)).ReturnsAsync(envio);
        _repoMock.Setup(x => x.GetCargaAsignadaAsync(1)).ReturnsAsync((0, 0m));

        // Mock genérico para cualquier cálculo de coordenadas que realice el servicio
        _distanciaServiceMock.Setup(x => x.CalcularPorCoordenadasAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
             .ReturnsAsync(new DistanciaRutaDto { DistanciaKm = 3.33, DuracionMinutos = 5 });

        _repoMock.Setup(x => x.AsignarEnvioAsync(100, 1, (byte)EstadoEnvio.ConductorAsignado))
             .ReturnsAsync(true);

        var resultado = await _service.AceptarOportunidadAsync(100, 1);

        Assert.True(resultado.Exito);
        Assert.Equal("Envío aceptado exitosamente.", resultado.Mensaje);
    }
}