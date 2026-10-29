using Microsoft.AspNetCore.Mvc;
using RuteoYa.Web.Logic;
using RuteoYa.Web.Models;

namespace RuteoYa.Web.Controllers;

public class UsuariosController : Controller
{
    private readonly UsuariosOrchestrator _orchestrator;

    public UsuariosController(UsuariosOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    // GET: /Usuarios
    public async Task<IActionResult> Index()
    {
        var usuarios = await _orchestrator.ObtenerTodosAsync();
        return View(usuarios);
    }

    // GET: /Usuarios/Registro
    public IActionResult Registro()
    {
        return View();
    }

    // POST: /Usuarios/Registro
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _orchestrator.RegistrarAsync(model);

        if (resultado.Success)
        {
            TempData["Success"] = resultado.Message;
            return RedirectToAction("Login");
        }

        ModelState.AddModelError("", resultado.Message);
        return View(model);
    }

    // GET: /Usuarios/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Usuarios/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _orchestrator.LoginAsync(model);

        if (resultado.Success && resultado.Usuario != null)
        {
            // TODO: Configurar autenticación con cookies/JWT
            // HttpContext.Session.SetInt32("UsuarioId", resultado.Usuario.Id);
            
            TempData["Success"] = $"Bienvenido {resultado.Usuario.NombreCompleto}";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", resultado.Message);
        return View(model);
    }

    // GET: /Usuarios/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var model = await _orchestrator.ObtenerParaEditarAsync(id);
        if (model == null)
        {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToAction("Index");
        }

        return View(model);
    }

    // POST: /Usuarios/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await _orchestrator.ActualizarAsync(model);

        if (resultado.Success)
        {
            TempData["Success"] = resultado.Message;
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", resultado.Message);
        return View(model);
    }

    // POST: /Usuarios/Eliminar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _orchestrator.EliminarAsync(id);

        if (resultado.Success)
        {
            TempData["Success"] = resultado.Message;
        }
        else
        {
            TempData["Error"] = resultado.Message;
        }

        return RedirectToAction("Index");
    }
}
