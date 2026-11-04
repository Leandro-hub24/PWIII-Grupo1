using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.API.Filters;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Services;
using RuteoYa.Logica.Services;
using System.Security.Claims;

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


        // GET: api/usuarios/mi-cuenta
        [HttpGet("mi-cuenta")]
        [Authorize]
        public async Task<ActionResult<Usuario>> GetUsuarioPorId()
        {
            // 1. Extraer el ID directamente desde los Claims del Token JWT
            string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int usuarioId))
            {
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario desde el token." });
            }

            var usuario = await _usuariosService.GetUsuarioPorIdAsync(usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        // POST: api/usuarios/create
        [HttpPost("create")]
        [NoAuthRequiredApi]
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

        // PUT: api/usuarios/mi-cuenta
        [HttpPut("mi-cuenta")]
        [Authorize]
        public async Task<ActionResult> UpdateUsuario([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            try
            {
                // 1. Extraer el ID directamente desde los Claims del Token JWT
                string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int usuarioId))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario desde el token." });
                }

                var actualizado = await _usuariosService.UpdateUsuarioAsync(usuarioId, usuario);

                return Ok(new { mensaje = "Tu cuenta ha sido actualizada exitosamente." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

           
        }

        // DELETE: api/usuarios/mi-cuenta
        [HttpDelete("mi-cuenta")]
        [Authorize]
        public async Task<IActionResult> EliminarMiCuenta()
        {
            try
            {
                // 1. Extraer el ID directamente desde los Claims del Token JWT
                string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int usuarioId))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario desde el token." });
                }

                // 2. Ejecutar la baja lógica o física en la base de datos usando ese ID seguro
                await _usuariosService.DeleteUsuarioAsync(usuarioId);

                return Ok(new { mensaje = "Tu cuenta ha sido eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
