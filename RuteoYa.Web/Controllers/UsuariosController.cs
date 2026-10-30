using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Web.Models;
using RuteoYa.Web.Services;

namespace RuteoYa.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }
        // GET: Usuarios
        public IActionResult Index()
        {
            return View();
        }

        // GET: Usuarios/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Crear()
        {
            ViewData["Title"] = "Crear cuenta";
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Crear", model);
            }

            try
            {
                var result = await _usuariosService.CreateUsuarioAsync(model);
                if (result)
                {
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    return View("Crear", model);
                }
            }
            catch
            {
                return View("Crear", model);
            }
        }

        // GET: UsuariosController/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: UsuariosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuariosController/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
