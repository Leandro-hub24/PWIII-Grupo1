// RuteoYa.API/Controllers/TestDistanciaController.cs

using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestDistanciaController : ControllerBase
{
    private readonly IDistanciaService _distanciaService;
    private readonly ILogger<TestDistanciaController> _logger;

    public TestDistanciaController(
        IDistanciaService distanciaService,
        ILogger<TestDistanciaController> logger)
    {
        _distanciaService = distanciaService;
        _logger = logger;
    }

    /// <summary>
    /// Prueba de cálculo de distancia y duración de ruta
    /// GET: api/testdistancia?origen=Obelisco, Buenos Aires&destino=Congreso, Buenos Aires
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TestCalcularRuta(
        [FromQuery] string origen, 
        [FromQuery] string destino)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(origen))
            {
                return BadRequest(new { error = "La dirección de origen es requerida" });
            }

            if (string.IsNullOrWhiteSpace(destino))
            {
                return BadRequest(new { error = "La dirección de destino es requerida" });
            }

            _logger.LogInformation("Calculando ruta de {Origen} a {Destino}", origen, destino);

            var resultado = await _distanciaService.CalcularAsync(origen, destino);

            return Ok(new
            {
                success = true,
                data = new
                {
                    origen = new
                    {
                        direccion = resultado.Origen.Direccion,
                        latitud = resultado.Origen.Lat,
                        longitud = resultado.Origen.Lng
                    },
                    destino = new
                    {
                        direccion = resultado.Destino.Direccion,
                        latitud = resultado.Destino.Lat,
                        longitud = resultado.Destino.Lng
                    },
                    distanciaKm = resultado.DistanciaKm,
                    duracionMinutos = resultado.DuracionMinutos,
                    duracionFormateada = FormatearDuracion(resultado.DuracionMinutos)
                },
                message = "Cálculo de ruta exitoso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular ruta");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                detail = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Prueba con rutas predefinidas
    /// GET: api/testdistancia/quick
    /// </summary>
    [HttpGet("quick")]
    public async Task<IActionResult> QuickTest()
    {
        var rutasTest = new[]
        {
            new { Origen = "Obelisco, Buenos Aires", Destino = "Congreso Nacional, Buenos Aires" },
            new { Origen = "Plaza de Mayo, Buenos Aires", Destino = "Recoleta, Buenos Aires" },
            new { Origen = "Torre Eiffel, Paris", Destino = "Louvre, Paris" }
        };

        var resultados = new List<object>();

        foreach (var ruta in rutasTest)
        {
            try
            {
                var resultado = await _distanciaService.CalcularAsync(ruta.Origen, ruta.Destino);
                resultados.Add(new
                {
                    origenSolicitado = ruta.Origen,
                    destinoSolicitado = ruta.Destino,
                    success = true,
                    resultado = new
                    {
                        origen = resultado.Origen.Direccion,
                        destino = resultado.Destino.Direccion,
                        distanciaKm = resultado.DistanciaKm,
                        duracionMinutos = resultado.DuracionMinutos,
                        duracionFormateada = FormatearDuracion(resultado.DuracionMinutos)
                    }
                });
            }
            catch (Exception ex)
            {
                resultados.Add(new
                {
                    origenSolicitado = ruta.Origen,
                    destinoSolicitado = ruta.Destino,
                    success = false,
                    error = ex.Message
                });
            }
        }

        return Ok(new
        {
            totalTests = rutasTest.Length,
            results = resultados
        });
    }

    /// <summary>
    /// Prueba específica con coordenadas conocidas (Buenos Aires)
    /// GET: api/testdistancia/buenosaires
    /// </summary>
    [HttpGet("buenosaires")]
    public async Task<IActionResult> TestBuenosAires()
    {
        try
        {
            var resultado = await _distanciaService.CalcularAsync(
                "Avenida Corrientes 1000, Buenos Aires",
                "Avenida Libertador 3000, Buenos Aires"
            );

            return Ok(new
            {
                success = true,
                descripcion = "Ruta típica en Buenos Aires",
                data = new
                {
                    origen = resultado.Origen,
                    destino = resultado.Destino,
                    distanciaKm = resultado.DistanciaKm,
                    duracionMinutos = resultado.DuracionMinutos,
                    duracionFormateada = FormatearDuracion(resultado.DuracionMinutos),
                    velocidadPromedioKmH = Math.Round((resultado.DistanciaKm / resultado.DuracionMinutos) * 60, 1)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en prueba de Buenos Aires");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Formatea minutos en formato legible (ej: "1h 23min")
    /// </summary>
    private string FormatearDuracion(int minutos)
    {
        if (minutos < 60)
        {
            return $"{minutos} min";
        }

        var horas = minutos / 60;
        var mins = minutos % 60;

        if (mins == 0)
        {
            return $"{horas}h";
        }

        return $"{horas}h {mins}min";
    }
}
