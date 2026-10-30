using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RuteoYa.Web.Models;
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
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TokenResponseDto?>();
    }

   
}
