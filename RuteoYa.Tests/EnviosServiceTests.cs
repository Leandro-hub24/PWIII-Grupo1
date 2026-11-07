using Moq;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.Common.Helpers;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;

namespace RuteoYa.Tests;

public class EnviosServiceTests
{
    private readonly Mock<IEnviosRepositorio> _repo = new();
    private readonly Mock<IGeocoderService> _geo = new();
    private readonly EnviosService _service;

    public EnviosServiceTests()
    {
        _geo.Setup(g => g.ObtenerCoordenadasAsync(It.IsAny<string>()))
            .ReturnsAsync((string d) => new GeocoderDto { Direccion = d, Lat = -34.6m, Lng = -58.5m });
        _service = new EnviosService(_repo.Object, _geo.Object);
    }

    private static CrearEnvioDto DtoValido() => new()
    {
        OrigenDir = "San Justo",
        DestinoDir = "Palermo",
        Fecha = DateTime.Today.AddDays(1),
        PesoKg = 2,
        Tamano = TamanoEnvio.Pequeno,
        Descripcion = "Libros",
        Monto = 5000
    };

    private static Envio EnvioDe(int usuarioId, EstadoEnvio estado) => new()
    {
        Id = 10,
        UsuarioId = usuarioId,
        OrigenDir = "A",
        DestinoDir = "B",
        Descripcion = "x",
        Tamano = (byte)TamanoEnvio.Mediano,
        Estado = (byte)estado
    };

    // ---------- Crear ----------

    [Fact]
    public async Task Crear_ConDatosValidos_QuedaBuscandoConductor()
    {
        var r = await _service.CrearEnvioAsync(1, DtoValido());

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        Assert.Equal(EstadoEnvio.BuscandoConductor, r.Valor!.Estado);
        _repo.Verify(x => x.CrearAsync(It.Is<Envio>(e => e.UsuarioId == 1 && e.AsignacionId == null)), Times.Once);
    }

    [Fact]
    public async Task Crear_OrigenIgualDestino_EsInvalido()
    {
        var dto = DtoValido();
        dto.DestinoDir = " san justo ";

        var r = await _service.CrearEnvioAsync(1, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
        _repo.Verify(x => x.CrearAsync(It.IsAny<Envio>()), Times.Never);
    }

    [Fact]
    public async Task Crear_FechaPasada_EsInvalido()
    {
        var dto = DtoValido();
        dto.Fecha = DateTime.Today.AddDays(-1);

        var r = await _service.CrearEnvioAsync(1, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Theory]
    [InlineData(0, 5000)]
    [InlineData(2, 0)]
    [InlineData(-1, 5000)]
    public async Task Crear_PesoOMontoNoPositivo_EsInvalido(decimal peso, decimal monto)
    {
        var dto = DtoValido();
        dto.PesoKg = peso;
        dto.Monto = monto;

        var r = await _service.CrearEnvioAsync(1, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Fact]
    public async Task Crear_SinTamanoNiDescripcion_EsInvalido()
    {
        var dto = DtoValido();
        dto.Tamano = null;
        Assert.Equal(TipoResultado.Invalido, (await _service.CrearEnvioAsync(1, dto)).Tipo);

        dto = DtoValido();
        dto.Descripcion = "  ";
        Assert.Equal(TipoResultado.Invalido, (await _service.CrearEnvioAsync(1, dto)).Tipo);
    }

    // ---------- Consultar ----------

    [Fact]
    public async Task GetEnvio_DeOtroUsuario_DevuelveNoEncontrado()
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(EnvioDe(usuarioId: 2, EstadoEnvio.BuscandoConductor));

        var r = await _service.GetEnvioAsync(usuarioId: 1, envioId: 10);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
    }

    [Fact]
    public async Task GetMisEnvios_PasaElFiltroAlRepositorio()
    {
        _repo.Setup(x => x.GetEnviosDeUsuarioAsync(1, EstadoEnvio.Cancelado))
             .ReturnsAsync(new List<Envio> { EnvioDe(1, EstadoEnvio.Cancelado) });

        var lista = await _service.GetMisEnviosAsync(1, EstadoEnvio.Cancelado);

        Assert.Single(lista);
        Assert.False(lista[0].SePuedeCancelar);
    }

    // ---------- Cancelar ----------

    [Theory]
    [InlineData(EstadoEnvio.BuscandoConductor)]
    [InlineData(EstadoEnvio.ConductorAsignado)]
    public async Task Cancelar_AntesDelRetiro_Funciona(EstadoEnvio estado)
    {
        var envio = EnvioDe(1, estado);
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(envio);

        var r = await _service.CancelarEnvioAsync(1, 10);

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        _repo.Verify(x => x.CambiarEstadoAsync(envio, EstadoEnvio.Cancelado), Times.Once);
    }

    [Theory]
    [InlineData(EstadoEnvio.Retirado)]
    [InlineData(EstadoEnvio.EnCamino)]
    [InlineData(EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.Cancelado)]
    public async Task Cancelar_DespuesDelRetiro_DaConflicto(EstadoEnvio estado)
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(EnvioDe(1, estado));

        var r = await _service.CancelarEnvioAsync(1, 10);

        Assert.Equal(TipoResultado.Conflicto, r.Tipo);
        _repo.Verify(x => x.CambiarEstadoAsync(It.IsAny<Envio>(), It.IsAny<EstadoEnvio>()), Times.Never);
    }

    [Fact]
    public async Task Cancelar_EnvioAjeno_DevuelveNoEncontrado()
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(EnvioDe(2, EstadoEnvio.BuscandoConductor));

        var r = await _service.CancelarEnvioAsync(1, 10);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
    }

    // ---------- Transiciones de estado ----------

    [Theory]
    [InlineData(EstadoEnvio.ConductorAsignado, EstadoEnvio.Retirado, true)]
    [InlineData(EstadoEnvio.Retirado, EstadoEnvio.EnCamino, true)]
    [InlineData(EstadoEnvio.EnCamino, EstadoEnvio.Entregado, true)]
    [InlineData(EstadoEnvio.ConductorAsignado, EstadoEnvio.Entregado, false)] // no entregar sin retirar
    [InlineData(EstadoEnvio.Entregado, EstadoEnvio.BuscandoConductor, false)] // entregado no vuelve a pendiente
    [InlineData(EstadoEnvio.Cancelado, EstadoEnvio.ConductorAsignado, false)] // cancelado no se asigna
    public void Transiciones_RespetanLasReglasDelEnunciado(EstadoEnvio actual, EstadoEnvio nuevo, bool esperado)
    {
        Assert.Equal(esperado, EstadoEnvioReglas.EsTransicionValida(actual, nuevo));
    }

    [Fact]
    public async Task CambiarEstado_Invalido_DaConflicto()
    {
        _repo.Setup(x => x.GetEnvioPorIdAsync(10)).ReturnsAsync(EnvioDe(1, EstadoEnvio.ConductorAsignado));

        var r = await _service.CambiarEstadoAsync(10, EstadoEnvio.Entregado);

        Assert.Equal(TipoResultado.Conflicto, r.Tipo);
    }
}
