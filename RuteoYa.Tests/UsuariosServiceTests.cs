using Moq;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Logica.Services;

namespace RuteoYa.Tests;

public class UsuariosServiceTests
{
    private readonly Mock<IUsuariosRepositorio> _repo = new();
    private readonly UsuariosService _service;

    public UsuariosServiceTests()
    {
        _service = new UsuariosService(_repo.Object);
    }

    private static Usuario UsuarioValido() => new()
    {
        Id = 1,
        Nombre = "Juan Perez",
        Email = "juan@example.com",
        PasswordHash = "Password123"
    };

    private static UsuarioDto UsuarioDtoValido() => new()
    {
        Id = 1,
        Nombre = "Juan",
        Apellido = "Perez",
        Email = "juan@example.com",
        PasswordHash = "Password123",
        CreadoEn = DateTime.UtcNow
    };

    // ---------- Crear ----------

    [Fact]
    public async Task CreateUsuario_ConDatosValidos_HasheaPasswordYGuarda()
    {
        var usuario = UsuarioValido();
        string passwordOriginal = usuario.PasswordHash;

        _repo.Setup(x => x.CreateUsuario(It.IsAny<Usuario>()))
             .ReturnsAsync(true);

        var resultado = await _service.CreateUsuarioAsync(usuario);

        Assert.True(resultado);
        Assert.NotEqual(passwordOriginal, usuario.PasswordHash);
        Assert.NotEqual(default(DateTime), usuario.CreadoEn);

        _repo.Verify(x => x.CreateUsuario(It.Is<Usuario>(u => u.Email == "juan@example.com")), Times.Once);
    }

    // ---------- Consultar ----------

    [Fact]
    public async Task GetUsuarioPorEmail_Existente_DevuelveUsuarioDto()
    {
        var usuarioDto = UsuarioDtoValido();
        _repo.Setup(x => x.GetUsuarioPorEmailAsync("juan@example.com"))
             .ReturnsAsync(usuarioDto);

        var resultado = await _service.GetUsuarioPorEmailAsync("juan@example.com");

        Assert.NotNull(resultado);
        Assert.Equal("juan@example.com", resultado.Email);
    }

    [Fact]
    public async Task GetUsuarioPorEmail_Inexistente_DevuelveNull()
    {
        _repo.Setup(x => x.GetUsuarioPorEmailAsync(It.IsAny<string>()))
             .ReturnsAsync((UsuarioDto?)null);

        var resultado = await _service.GetUsuarioPorEmailAsync("noexiste@example.com");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetUsuarioPorId_Existente_DevuelveUsuario()
    {
        var usuario = UsuarioValido();
        _repo.Setup(x => x.GetUsuarioPorId(1))
             .ReturnsAsync(usuario);

        var resultado = await _service.GetUsuarioPorIdAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
    }

    [Fact]
    public async Task GetUsuarios_DevuelveListaDeUsuarios()
    {
        var listaUsuarios = new List<Usuario> { UsuarioValido() };
        _repo.Setup(x => x.GetUsuarios())
             .ReturnsAsync(listaUsuarios);

        var resultado = await _service.GetUsuariosAsync();

        Assert.Single(resultado);
    }

    // ---------- Actualizar y Eliminar ----------

    [Fact]
    public async Task UpdateUsuario_LlamaAlRepositorioYDevuelveTrue()
    {
        var usuario = UsuarioValido();
        _repo.Setup(x => x.UpdateUsuario(1, usuario))
             .ReturnsAsync(true);

        var resultado = await _service.UpdateUsuarioAsync(1, usuario);

        Assert.True(resultado);
        _repo.Verify(x => x.UpdateUsuario(1, usuario), Times.Once);
    }

    [Fact]
    public async Task DeleteUsuario_LlamaAlRepositorioYDevuelveTrue()
    {
        _repo.Setup(x => x.DeleteUsuario(1))
             .ReturnsAsync(true);

        var resultado = await _service.DeleteUsuarioAsync(1);

        Assert.True(resultado);
        _repo.Verify(x => x.DeleteUsuario(1), Times.Once);
    }
}