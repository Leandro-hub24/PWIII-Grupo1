using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;

namespace RuteoYa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosLogica;

        public UsuariosController(IUsuariosService usuariosLogica)
        {
            _usuariosLogica = usuariosLogica;
        }

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            var usuarios = await _usuariosLogica.GetUsuariosAsync();
            return Ok(usuarios);
        }

        // GET: api/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetUsuarioPorId(int id)
        {
            var usuario = await _usuariosLogica.GetUsuarioPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        // POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult> CreateUsuario([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var creado = await _usuariosLogica.CreateUsuarioAsync(usuario);

            if (!creado)
            {
                return BadRequest("No se pudo crear el usuario.");
            }

            // Retorna un código 201 Created apuntando al endpoint de consulta por ID
            return CreatedAtAction(nameof(GetUsuarioPorId), new { id = usuario.Id }, usuario);
        }

        // PUT: api/usuarios/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateUsuario(int id, [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var actualizado = await _usuariosLogica.UpdateUsuarioAsync(id, usuario);

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
            var eliminado = await _usuariosLogica.DeleteUsuarioAsync(id);

            if (!eliminado)
            {
                return NotFound($"No se encontró el usuario con ID {id} para eliminar.");
            }

            return NoContent();
        }
    }
}
