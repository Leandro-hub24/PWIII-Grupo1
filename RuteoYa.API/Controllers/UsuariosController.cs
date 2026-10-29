using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// GET: api/usuarios
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> ObtenerTodos()
    {
        var usuarios = await _usuarioService.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// GET: api/usuarios/5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(usuario);
    }

    /// <summary>
    /// POST: api/usuarios
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear([FromBody] CrearUsuarioDto dto)
    {
        var resultado = await _usuarioService.CrearAsync(dto);

        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return CreatedAtAction(nameof(ObtenerPorId), 
            new { id = resultado.Usuario!.Id }, 
            resultado.Usuario);
    }

    /// <summary>
    /// PUT: api/usuarios/5
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "El ID de la URL no coincide con el del body" });

        var resultado = await _usuarioService.ActualizarAsync(dto);

        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return NoContent();
    }

    /// <summary>
    /// DELETE: api/usuarios/5
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var resultado = await _usuarioService.EliminarAsync(id);

        if (!resultado.Success)
            return NotFound(new { message = resultado.Message });

        return NoContent();
    }

    /// <summary>
    /// POST: api/usuarios/validar
    /// </summary>
    [HttpPost("validar")]
    public async Task<ActionResult<UsuarioDto>> ValidarCredenciales([FromBody] ValidarCredencialesRequest request)
    {
        var esValido = await _usuarioService.ValidarCredencialesAsync(request.Email, request.Password);

        if (!esValido)
            return Unauthorized(new { message = "Credenciales inválidas" });

        var usuario = await _usuarioService.ObtenerPorEmailAsync(request.Email);
        return Ok(usuario);
    }
}

public record ValidarCredencialesRequest(string Email, string Password);
