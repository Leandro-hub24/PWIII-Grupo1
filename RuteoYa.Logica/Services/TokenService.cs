using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RuteoYa.Entidades.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerarTokenJwt(UsuarioDto usuario)
    {
        // 1. Definir los claims (datos que viajarán dentro del token)
        var claims = new List<Claim>
        {
            // Claims estándar de .NET
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            
            // Claims personalizados (Nombre y Apellido)
            new Claim("Nombre", usuario.Nombre),
            new Claim("Apellido", usuario.Apellido),
            new Claim("NombreCompleto", $"{usuario.Nombre} {usuario.Apellido}")
        };

        // 2. Obtener la clave secreta desde appsettings.json
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Configurar los datos del token (Emisor, Audiencia, Claims, Expiración)
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // Tiempo de vida del token
            signingCredentials: creds
        );

        // 4. Retornar el token convertido en cadena de texto (string)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}