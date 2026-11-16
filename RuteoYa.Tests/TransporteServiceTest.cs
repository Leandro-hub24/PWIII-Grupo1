using Moq;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Logica.Services;

namespace RuteoYa.Tests;

public class TransportesServiceTests
{
    private readonly Mock<IEnviosRepositorio> _repo = new();
    private readonly TransportesService _service;

    public TransportesServiceTests()
    {
        _service = new TransportesService(_repo.Object);
    }

    // Envío asignado a un viaje cuyo conductor es conductorId.
    private static Envio TransporteDe(int conductorId, EstadoEnvio estado) => new()
    {
        Id = 10,
        UsuarioId = 99,
        OrigenDir = "A",
        DestinoDir = "B",
        Descripcion = "Zapatillas",
        Tamano = (byte)TamanoEnvio.Mediano,
        Estado = (byte)estado,
        Asignacion = new Asignacion
        {
            Id = 1,
            ViajeId = 5,
            FechaViaje = DateTime.Today,
            Ganancia = 4000,
            Viaje = new Viaje { Id = 5, UsuarioId = conductorId, OrigenDir = "Belgrano", DestinoDir = "Lomas" }
        }
    };

    // ---------- Listar ----------

    [Fact]
    public async Task GetMisTransportes_PasaElFiltroYCalculaElSiguienteEstado()
    {
        _repo.Setup(x => x.GetTransportesDeConductorAsync(1, EstadoEnvio.Retirado))
             .ReturnsAsync(new List<Envio> { TransporteDe(1, EstadoEnvio.Retirado) });

        var lista = await _service.GetMisTransportesAsync(1, EstadoEnvio.Retirado);

        Assert.Single(lista);
        Assert.Equal(4000, lista[0].Ganancia);
        Assert.Equal(EstadoEnvio.EnCamino, lista[0].EstadoSiguiente);
    }

    [Fact]
    public async Task GetMisTransportes_Entregado_NoTieneSiguienteEstado()
    {
        _repo.Setup(x => x.GetTransportesDeConductorAsync(1, null))
             .ReturnsAsync(new List<Envio> { TransporteDe(1, EstadoEnvio.Entregado) });

        var lista = await _service.GetMisTransportesAsync(1, null);

        Assert.Null(lista[0].EstadoSiguiente);
    }

    // ---------- Cambiar estado ----------

    [Theory]
    [InlineData(EstadoEnvio.ConductorAsignado, EstadoEnvio.Retirado)]
    [InlineData(EstadoEnvio.Retirado, EstadoEnvio.EnCamino)]
    [InlineData(EstadoEnvio.EnCamino, EstadoEnvio.Entregado)]
    public async Task CambiarEstado_TransicionValida_Funciona(EstadoEnvio actual, EstadoEnvio nuevo)
    {
        var envio = TransporteDe(1, actual);
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(envio);

        var r = await _service.CambiarEstadoAsync(1, 10, nuevo);

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        _repo.Verify(x => x.CambiarEstadoAsync(envio, nuevo), Times.Once);
    }

    [Fact]
    public async Task CambiarEstado_ConductorAjeno_DevuelveNoEncontrado()
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(TransporteDe(conductorId: 2, EstadoEnvio.ConductorAsignado));

        var r = await _service.CambiarEstadoAsync(1, 10, EstadoEnvio.Retirado);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
        _repo.Verify(x => x.CambiarEstadoAsync(It.IsAny<Envio>(), It.IsAny<EstadoEnvio>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstado_EnvioSinAsignacion_DevuelveNoEncontrado()
    {
        var sinAsignar = TransporteDe(1, EstadoEnvio.BuscandoConductor);
        sinAsignar.Asignacion = null;
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(sinAsignar);

        var r = await _service.CambiarEstadoAsync(1, 10, EstadoEnvio.Retirado);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
    }

    [Fact]
    public async Task CambiarEstado_EnvioInexistente_DevuelveNoEncontrado()
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync((Envio?)null);

        var r = await _service.CambiarEstadoAsync(1, 10, EstadoEnvio.Retirado);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
    }

    [Fact]
    public async Task CambiarEstado_EntregarSinRetirar_DaConflicto()
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(TransporteDe(1, EstadoEnvio.ConductorAsignado));

        var r = await _service.CambiarEstadoAsync(1, 10, EstadoEnvio.Entregado);

        Assert.Equal(TipoResultado.Conflicto, r.Tipo);
        _repo.Verify(x => x.CambiarEstadoAsync(It.IsAny<Envio>(), It.IsAny<EstadoEnvio>()), Times.Never);
    }

    [Theory]
    [InlineData(EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.Cancelado)]
    public async Task CambiarEstado_DesdeEstadoFinal_DaConflicto(EstadoEnvio final)
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(TransporteDe(1, final));

        var r = await _service.CambiarEstadoAsync(1, 10, EstadoEnvio.EnCamino);

        Assert.Equal(TipoResultado.Conflicto, r.Tipo);
    }

    [Theory]
    [InlineData(EstadoEnvio.Cancelado)]
    [InlineData(EstadoEnvio.ConductorAsignado)]
    [InlineData(EstadoEnvio.BuscandoConductor)]
    public async Task CambiarEstado_EstadoNoPermitidoParaConductor_EsInvalido(EstadoEnvio nuevo)
    {
        var r = await _service.CambiarEstadoAsync(1, 10, nuevo);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
        _repo.Verify(x => x.GetEnvioPorIdAsync(It.IsAny<int>()), Times.Never);
    }
}