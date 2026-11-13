using Moq;
using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;

namespace RuteoYa.Tests;

public class ViajesServiceTests
{
    private const int Dueno = 1;
    private const int OtroUsuario = 2;

    private readonly Mock<IViajesRepositorio> _repo = new();
    private readonly Mock<IDistanciaService> _distancia = new();
    private readonly ViajesService _service;

    public ViajesServiceTests()
    {
        _distancia.Setup(d => d.CalcularAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string origen, string destino) => new DistanciaRutaDto
            {
                Origen = new GeocoderDto { Direccion = origen, Lat = -34.6m, Lng = -58.5m },
                Destino = new GeocoderDto { Direccion = destino, Lat = -34.7m, Lng = -58.4m },
                DistanciaKm = 10,
                DuracionMinutos = 20
            });
        _repo.Setup(r => r.GetEnviosAsignadosAsync(It.IsAny<int>())).ReturnsAsync(new List<Envio>());
        _service = new ViajesService(_repo.Object, _distancia.Object);
    }

    private static CrearViajeDto DtoValido() => new()
    {
        OrigenDir = "San Justo",
        DestinoDir = "Palermo",
        HoraSalida = new TimeOnly(8, 0),
        HoraLlegada = new TimeOnly(9, 30),
        MaxPaquetes = 3,
        PesoMaxKg = 10,
        DesvioMaxKm = 5,
        Dias = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday }
    };

    private static Viaje ViajeDe(int usuarioId, bool activo = true)
    {
        var viaje = new Viaje
        {
            Id = 10,
            UsuarioId = usuarioId,
            OrigenDir = "A",
            DestinoDir = "B",
            Activo = activo
        };
        viaje.ViajeDias.Add(new ViajeDia { DiaSemana = (byte)DayOfWeek.Friday });
        viaje.Paradas.Add(new Parada { Orden = 1, Direccion = "Parada vieja" });
        return viaje;
    }

    [Fact]
    public async Task Crear_ConDatosValidos_GuardaElViajeActivoDelUsuario()
    {
        var r = await _service.CrearViajeAsync(Dueno, DtoValido());

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        Assert.True(r.Valor!.Activo);
        Assert.Equal("San Justo", r.Valor.OrigenDir);
        Assert.Equal("Palermo", r.Valor.DestinoDir);
        Assert.Equal(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }, r.Valor.Dias);
        _repo.Verify(x => x.CrearAsync(It.Is<Viaje>(v =>
            v.UsuarioId == Dueno && v.DestinoDir == "Palermo" && v.ViajeDias.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task Crear_ConParadas_SumaLaDistanciaDeCadaTramoYRespetaElOrden()
    {
        var dto = DtoValido();
        dto.Paradas = new List<CrearParadaDto>
        {
            new() { Direccion = "Flores", Orden = 2 },
            new() { Direccion = "Ramos Mejía", Orden = 1 }
        };

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(30, r.Valor!.DistanciaKm);
        Assert.Equal(new[] { "Ramos Mejía", "Flores" }, r.Valor.Paradas.Select(p => p.Direccion));
        Assert.Equal(new[] { 1, 2 }, r.Valor.Paradas.Select(p => p.Orden));
        _distancia.Verify(d => d.CalcularAsync("San Justo", "Ramos Mejía"), Times.Once);
        _distancia.Verify(d => d.CalcularAsync("Flores", "Palermo"), Times.Once);
    }

    [Fact]
    public async Task Crear_ConOrigenIgualAlDestino_EsInvalido()
    {
        var dto = DtoValido();
        dto.DestinoDir = "  san justo ";

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
        _repo.Verify(x => x.CrearAsync(It.IsAny<Viaje>()), Times.Never);
    }

    [Fact]
    public async Task Crear_SinDias_EsInvalido()
    {
        var dto = DtoValido();
        dto.Dias.Clear();

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Fact]
    public async Task Crear_ConLlegadaAntesDeLaSalida_EsInvalido()
    {
        var dto = DtoValido();
        dto.HoraLlegada = new TimeOnly(7, 0);

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(3, 0)]
    public async Task Crear_ConCapacidadOPesoEnCero_EsInvalido(int maxPaquetes, decimal pesoMaxKg)
    {
        var dto = DtoValido();
        dto.MaxPaquetes = maxPaquetes;
        dto.PesoMaxKg = pesoMaxKg;

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Fact]
    public async Task Crear_ConParadasRepetidas_EsInvalido()
    {
        var dto = DtoValido();
        dto.Paradas = new List<CrearParadaDto>
        {
            new() { Direccion = "Flores", Orden = 1 },
            new() { Direccion = "FLORES", Orden = 2 }
        };

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Theory]
    [InlineData("San Justo")]
    [InlineData("Palermo")]
    public async Task Crear_ConParadaIgualAOrigenODestino_EsInvalido(string parada)
    {
        var dto = DtoValido();
        dto.Paradas = new List<CrearParadaDto> { new() { Direccion = parada, Orden = 1 } };

        var r = await _service.CrearViajeAsync(Dueno, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
    }

    [Fact]
    public async Task GetViaje_DeOtroUsuario_DevuelveNoEncontrado()
    {
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(ViajeDe(Dueno));

        var r = await _service.GetViajeAsync(OtroUsuario, 10);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
    }

    [Fact]
    public async Task GetViaje_Propio_IncluyeLosPaquetesAsignados()
    {
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(ViajeDe(Dueno));
        _repo.Setup(x => x.GetEnviosAsignadosAsync(10)).ReturnsAsync(new List<Envio>
        {
            new()
            {
                Id = 7, OrigenDir = "A", DestinoDir = "B", Descripcion = "Libros", PesoKg = 2, Monto = 4000,
                Estado = (byte)EstadoEnvio.ConductorAsignado,
                Asignacion = new Asignacion { ViajeId = 10, FechaViaje = new DateTime(2026, 11, 20) }
            }
        });

        var r = await _service.GetViajeAsync(Dueno, 10);

        var paquete = Assert.Single(r.Valor!.PaquetesAsignados);
        Assert.Equal(7, paquete.EnvioId);
        Assert.Equal("Conductor asignado", paquete.Estado);
    }

    [Fact]
    public async Task GetMisViajes_PasaElFiltroDeEstadoAlRepositorio()
    {
        _repo.Setup(x => x.GetViajesDeUsuarioAsync(Dueno, false)).ReturnsAsync(new List<Viaje> { ViajeDe(Dueno, activo: false) });

        var viajes = await _service.GetMisViajesAsync(Dueno, false);

        Assert.False(Assert.Single(viajes).Activo);
    }

    [Fact]
    public async Task Actualizar_DeOtroUsuario_DevuelveNoEncontradoYNoGuarda()
    {
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(ViajeDe(Dueno));

        var r = await _service.ActualizarViajeAsync(OtroUsuario, 10, DtoValido());

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
        _repo.Verify(x => x.ActualizarAsync(It.IsAny<Viaje>()), Times.Never);
    }

    [Fact]
    public async Task Actualizar_Propio_ReemplazaDatosDiasYParadas()
    {
        var viaje = ViajeDe(Dueno);
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(viaje);
        var dto = DtoValido();
        dto.Paradas = new List<CrearParadaDto> { new() { Direccion = "Flores", Orden = 1 } };

        var r = await _service.ActualizarViajeAsync(Dueno, 10, dto);

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        Assert.Equal("Palermo", viaje.DestinoDir);
        Assert.Equal("Flores", Assert.Single(viaje.Paradas).Direccion);
        Assert.Equal(2, viaje.ViajeDias.Count);
        _repo.Verify(x => x.ActualizarAsync(viaje), Times.Once);
    }

    [Fact]
    public async Task Actualizar_ConDatosInvalidos_NoModificaElViaje()
    {
        var viaje = ViajeDe(Dueno);
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(viaje);
        var dto = DtoValido();
        dto.Dias.Clear();

        var r = await _service.ActualizarViajeAsync(Dueno, 10, dto);

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
        Assert.Equal("Parada vieja", Assert.Single(viaje.Paradas).Direccion);
        _repo.Verify(x => x.ActualizarAsync(It.IsAny<Viaje>()), Times.Never);
    }

    [Fact]
    public async Task Desactivar_ViajeActivoPropio_LoDejaInactivo()
    {
        var viaje = ViajeDe(Dueno);
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(viaje);

        var r = await _service.DesactivarViajeAsync(Dueno, 10);

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        Assert.False(viaje.Activo);
    }

    [Fact]
    public async Task Desactivar_ViajeYaInactivo_DevuelveConflicto()
    {
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(ViajeDe(Dueno, activo: false));

        var r = await _service.DesactivarViajeAsync(Dueno, 10);

        Assert.Equal(TipoResultado.Conflicto, r.Tipo);
    }

    [Fact]
    public async Task Desactivar_ViajeDeOtroUsuario_DevuelveNoEncontrado()
    {
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(ViajeDe(Dueno));

        var r = await _service.DesactivarViajeAsync(OtroUsuario, 10);

        Assert.Equal(TipoResultado.NoEncontrado, r.Tipo);
    }

    [Fact]
    public async Task Activar_ViajeYaActivo_DevuelveConflicto()
    {
        _repo.Setup(x => x.GetViajePorIdAsync(10)).ReturnsAsync(ViajeDe(Dueno));

        var r = await _service.ActivarViajeAsync(Dueno, 10);

        Assert.Equal(TipoResultado.Conflicto, r.Tipo);
    }

    [Fact]
    public async Task CalcularRuta_ConUnaParada_DevuelveDosTramosYTresPuntos()
    {
        var r = await _service.CalcularRutaAsync(new CalcularRutaDto
        {
            OrigenDir = "San Justo",
            DestinoDir = "Palermo",
            Paradas = new List<string> { "Flores", "  " }
        });

        Assert.Equal(TipoResultado.Ok, r.Tipo);
        Assert.Equal(20, r.Valor!.DistanciaKm);
        Assert.Equal(40, r.Valor.DuracionMinutos);
        Assert.Equal(3, r.Valor.Puntos.Count);
    }

    [Fact]
    public async Task CalcularRuta_ConOrigenIgualAlDestino_EsInvalido()
    {
        var r = await _service.CalcularRutaAsync(new CalcularRutaDto { OrigenDir = "Palermo", DestinoDir = "Palermo" });

        Assert.Equal(TipoResultado.Invalido, r.Tipo);
        _distancia.Verify(d => d.CalcularAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
