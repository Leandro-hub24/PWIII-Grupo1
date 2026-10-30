using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrototipoDeRuteaYa.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PrototipoDeRuteaYa.Controllers
{
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
            ViewData["Title"] = "Home";
            int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string emailUsuario = User.FindFirstValue(ClaimTypes.Name);
            ViewData["nombreUsuario"] = emailUsuario ?? "Invitado";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
