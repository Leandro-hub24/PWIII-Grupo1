using Microsoft.AspNetCore.Mvc;

namespace PrototipoDeRuteaYa.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Ingresar";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Redirige al Dashboard (Home/Index) al presionar "Ingresar"
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Crear cuenta";
            return View();
        }

        [HttpPost]
        public IActionResult Register(string nombre, string apellido, string email, string password)
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}