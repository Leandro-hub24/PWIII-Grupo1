using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;

namespace RuteoYa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            var usuarios = await _usuariosService.GetUsuariosAsync();
            return Ok(usuarios);
        }
       

        // GET: api/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetUsuarioPorId(int id)
        {
            var usuario = await _usuariosService.GetUsuarioPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        // POST: api/usuarios/create
        [HttpPost("create")]
        public async Task<ActionResult<bool>> CreateUsuario([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var creado = await _usuariosService.CreateUsuarioAsync(usuario);

                if (!creado)
                {
                    return BadRequest("No se pudo crear el usuario en la base de datos.");
                }

                return Ok(true);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, "Error interno del servidor.");
            }
      
        }

        // PUT: api/usuarios/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateUsuario(int id, [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var actualizado = await _usuariosService.UpdateUsuarioAsync(id, usuario);

            if (!actualizado)
            {
                return NotFound($"No se encontró el usuario con ID {id} para actualizar.");
            }

            return NoContent(); // 204 No Content indica éxito sin necesidad de retornar cuerpo
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            var eliminado = await _usuariosService.DeleteUsuarioAsync(id);

            if (!eliminado)
            {
                return NotFound($"No se encontró el usuario con ID {id} para eliminar.");
            }

            return NoContent();
        }
    }
}
