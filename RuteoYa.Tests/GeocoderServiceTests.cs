using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Logica.Services;
using System.Net;
using System.Text.Json;

namespace RuteoYa.Tests;

public class GeocoderServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _configuration;
    private readonly GeocoderService _service;

    public GeocoderServiceTests()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object);
        _configuration = new Mock<IConfiguration>();
        _configuration.Setup(c => c["HereApi:ApiKey"]).Returns("test-api-key");
        _service = new GeocoderService(_httpClient, _configuration.Object);
    }

    [Fact]
    public async Task ObtenerCoordenadasAsync_ConDireccionValida_RetornaCoordenadas()
    {
        // Arrange
        var direccion = "Av. Corrientes 1234, Buenos Aires";
        var responseJson = @"{
            ""items"": [
                {
                    ""title"": ""Av. Corrientes 1234"",
                    ""address"": {
                        ""label"": ""Av. Corrientes 1234, Buenos Aires, Argentina"",
                        ""city"": ""Buenos Aires"",
                        ""street"": ""Av. Corrientes"",
                        ""postalCode"": ""C1043""
                    },
                    ""position"": {
                        ""lat"": -34.603722,
                        ""lng"": -58.381592
                    }
                }
            ]
        }";

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("geocode.search.hereapi.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var resultado = await _service.ObtenerCoordenadasAsync(direccion);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Av. Corrientes 1234, Buenos Aires, Argentina", resultado.Direccion);
        Assert.Equal(-34.603722m, resultado.Lat);
        Assert.Equal(-58.381592m, resultado.Lng);
    }

    [Fact]
    public async Task ObtenerCoordenadasAsync_ConDireccionConEspacios_CodificaCorrectamente()
    {
        // Arrange
        var direccion = "5 Rue Daunou, Paris";
        var responseJson = @"{
            ""items"": [
                {
                    ""title"": ""5 Rue Daunou"",
                    ""address"": {
                        ""label"": ""5 Rue Daunou, 75002 Paris, France""
                    },
                    ""position"": {
                        ""lat"": 48.869445,
                        ""lng"": 2.332896
                    }
                }
            ]
        }";

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var resultado = await _service.ObtenerCoordenadasAsync(direccion);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(48.869445m, resultado.Lat);
        Assert.Equal(2.332896m, resultado.Lng);
    }

    [Fact]
    public async Task ObtenerCoordenadasAsync_ConDireccionInexistente_LanzaExcepcion()
    {
        // Arrange
        var direccion = "Direccion Inexistente 99999";
        var responseJson = @"{""items"": []}";

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _service.ObtenerCoordenadasAsync(direccion));
    }

    [Fact]
    public async Task ObtenerCoordenadasAsync_ConErrorDeAPI_LanzaExcepcion()
    {
        // Arrange
        var direccion = "Test Address";

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _service.ObtenerCoordenadasAsync(direccion));
    }

    [Fact]
    public async Task ObtenerCoordenadasAsync_SinApiKey_LanzaExcepcion()
    {
        // Arrange
        var configSinKey = new Mock<IConfiguration>();
        configSinKey.Setup(c => c["HereApi:ApiKey"]).Returns((string?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => new GeocoderService(_httpClient, configSinKey.Object));
    }

    [Fact]
    public async Task ObtenerCoordenadasAsync_ConCaracteresEspeciales_CodificaCorrectamente()
    {
        // Arrange
        var direccion = "Calle O'Higgins #123, Santiago";
        var responseJson = @"{
            ""items"": [
                {
                    ""title"": ""Calle O'Higgins 123"",
                    ""address"": {
                        ""label"": ""Calle O'Higgins 123, Santiago, Chile""
                    },
                    ""position"": {
                        ""lat"": -33.4489,
                        ""lng"": -70.6693
                    }
                }
            ]
        }";

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var resultado = await _service.ObtenerCoordenadasAsync(direccion);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(-33.4489m, resultado.Lat);
        Assert.Equal(-70.6693m, resultado.Lng);
    }
}
