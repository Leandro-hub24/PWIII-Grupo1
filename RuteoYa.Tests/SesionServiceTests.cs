using Moq;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Logica.Services;

namespace RuteoYa.Tests;

public class SesionServiceTests
{
    private readonly Mock<IUsuariosRepositorio> _repo = new();
    private readonly SesionService _service;

    public SesionServiceTests()
    {
        _service = new SesionService(_repo.Object);
    }

    private static UsuarioDto UsuarioDtoEjemplo(string passwordHash) => new()
    {
        Id = 1,
        Nombre = "Juan",
        Apellido = "Perez",
        Email = "juan@example.com",
        PasswordHash = passwordHash,
        CreadoEn = DateTime.Now
    };

    [Fact]
    public async Task IniciarSesion_ConCredencialesValidas_DevuelveUsuarioDto()
    {
        // Arrange
        string passwordPlano = "Password123";
        string passwordHasheado = BCrypt.Net.BCrypt.HashPassword(passwordPlano);

        var usuarioDto = UsuarioDtoEjemplo(passwordHasheado);

        // El repositorio devuelve UsuarioDto? según tu interfaz
        _repo.Setup(x => x.GetUsuarioPorEmailAsync("juan@example.com"))
             .ReturnsAsync(usuarioDto);

        var dto = new IniciarSesionDto
        {
            Email = "juan@example.com",
            Password = passwordPlano
        };

        // Act
        var resultado = await _service.IniciarSesionAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuarioDto.Email, resultado.Email);
        Assert.Equal(usuarioDto.Nombre, resultado.Nombre);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioNoEncontrado_LanzaExcepcion()
    {
        // Arrange
        _repo.Setup(x => x.GetUsuarioPorEmailAsync(It.IsAny<string>()))
             .ReturnsAsync((UsuarioDto?)null);

        var dto = new IniciarSesionDto
        {
            Email = "noexiste@example.com",
            Password = "Password123"
        };

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<Exception>(() => _service.IniciarSesionAsync(dto));
        Assert.Equal("Usuario no encontrado.", excepcion.Message);
    }

    [Fact]
    public async Task IniciarSesion_PasswordIncorrecto_LanzaExcepcion()
    {
        // Arrange
        string passwordReal = "Password123";
        string passwordHasheado = BCrypt.Net.BCrypt.HashPassword(passwordReal);

        var usuarioDto = UsuarioDtoEjemplo(passwordHasheado);

        _repo.Setup(x => x.GetUsuarioPorEmailAsync("juan@example.com"))
             .ReturnsAsync(usuarioDto);

        var dto = new IniciarSesionDto
        {
            Email = "juan@example.com",
            Password = "PasswordIncorrecto"
        };

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<Exception>(() => _service.IniciarSesionAsync(dto));
        Assert.Equal("Credenciales incorrectas.", excepcion.Message);
    }
}