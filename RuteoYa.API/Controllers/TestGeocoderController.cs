// RuteoYa.API/Controllers/TestGeocoderController.cs

using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestGeocoderController : ControllerBase
{
    private readonly IGeocoderService _geocoderService;
    private readonly ILogger<TestGeocoderController> _logger;

    public TestGeocoderController(
        IGeocoderService geocoderService,
        ILogger<TestGeocoderController> logger)
    {
        _geocoderService = geocoderService;
        _logger = logger;
    }

    /// <summary>
    /// Prueba de geocodificación
    /// GET: api/testgeocoder?direccion=Av Corrientes 1000, Buenos Aires
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TestGeocoding([FromQuery] string direccion)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(direccion))
            {
                return BadRequest(new { error = "La dirección es requerida" });
            }

            _logger.LogInformation("Testing geocodificación para: {Direccion}", direccion);

            var resultado = await _geocoderService.ObtenerCoordenadasAsync(direccion);

            return Ok(new
            {
                success = true,
                data = resultado,
                message = "Geocodificación exitosa"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al geocodificar");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                detail = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Prueba con dirección predefinida
    /// GET: api/testgeocoder/quick
    /// </summary>
    [HttpGet("quick")]
    public async Task<IActionResult> QuickTest()
    {
        var direccionesTest = new[]
        {
            "Av Corrientes 1000, Buenos Aires, Argentina",
            "Obelisco, Buenos Aires",
            "Torre Eiffel, Paris"
        };

        var resultados = new List<object>();

        foreach (var dir in direccionesTest)
        {
            try
            {
                var resultado = await _geocoderService.ObtenerCoordenadasAsync(dir);
                resultados.Add(new
                {
                    direccionOriginal = dir,
                    success = true,
                    resultado
                });
            }
            catch (Exception ex)
            {
                resultados.Add(new
                {
                    direccionOriginal = dir,
                    success = false,
                    error = ex.Message
                });
            }
        }

        return Ok(new
        {
            totalTests = direccionesTest.Length,
            results = resultados
        });
    }
}
