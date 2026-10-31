using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RuteoYa.Web.Models;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace RuteoYa.Web.Services;

public interface IAuthService
{
    Task<TokenResponseDto?> LoginAsync(LoginViewModel model);
    ClaimsIdentity CrearClaims(TokenResponseDto token);
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MiApi");
    }

    public ClaimsIdentity CrearClaims(TokenResponseDto token)
    {
        // 2. Crear los claims del usuario para el MVC (puedes decodificar el JWT o usar datos que devuelva la API)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, token.Id.ToString()),
            new Claim(ClaimTypes.Name, token.NombreApellido),
            // Guardamos el token JWT dentro de un claim para usarlo después al llamar a la API
            new Claim("JwtToken", token.Token)
        };
        return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sesion/login", model);

        if(!response.IsSuccessStatusCode)
        {
            // Lee el error exacto que devuelve la API para saber qué falló
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error de API: {response.StatusCode} - {errorContent}");
            if(response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception("Credenciales incorrectas.");
            }

            if(response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocurrió un error inesperado al iniciar sesión. Intente más tarde.");
            }
        }

        return await response.Content.ReadFromJsonAsync<TokenResponseDto?>();
    }

   
}
