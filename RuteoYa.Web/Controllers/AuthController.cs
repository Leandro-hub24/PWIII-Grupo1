using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using RuteoYa.Web.Models;
using RuteoYa.Web.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace PrototipoDeRuteaYa.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Ingresar";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resultado = await _authService.LoginAsync(model);

                var claimsIdentity = _authService.CrearClaims(resultado);

                // 3. Registrar al usuario en el contexto de ASP.NET Core MVC (Crea la Cookie HttpOnly automáticamente)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction( "Index","Home");
                }
            catch (Exception ex) 
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Crear cuenta";
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}