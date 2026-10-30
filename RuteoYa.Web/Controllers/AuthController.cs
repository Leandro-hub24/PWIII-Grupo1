using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RuteoYa.Web.Models;
using System.Security.Claims;

namespace PrototipoDeRuteaYa.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MiApi");
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

            // 1. Consumir la Web API para validar credenciales y obtener el JWT
            var response = await _httpClient.PostAsJsonAsync("api/sesion/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
                return View(model);
            }

            var resultado = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

            // 2. Crear los claims del usuario para el MVC (puedes decodificar el JWT o usar datos que devuelva la API)
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, resultado.Id.ToString()),
            new Claim(ClaimTypes.Name, resultado.NombreApellido),
            // Guardamos el token JWT dentro de un claim para usarlo después al llamar a la API
            new Claim("JwtToken", resultado.Token)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Registrar al usuario en el contexto de ASP.NET Core MVC (Crea la Cookie HttpOnly automáticamente)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction( "Index","Home");
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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}