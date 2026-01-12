using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Aplicacion.Consultas;
using Usuarios.Aplicacion.DTOs;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;

namespace Usuarios.Pruebas.Aplicacion;

public class ConsultarUsuarioQueryHandlerTests
{
    private readonly Mock<IRepositorioUsuarios> _mockRepositorio;
    private readonly Mock<ILogger<ConsultarUsuarioQueryHandler>> _mockLogger;
    private readonly ConsultarUsuarioQueryHandler _handler;

    public ConsultarUsuarioQueryHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioUsuarios>();
        _mockLogger = new Mock<ILogger<ConsultarUsuarioQueryHandler>>();
        
        _handler = new ConsultarUsuarioQueryHandler(
            _mockRepositorio.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ConUsuarioExistenteYActivo_RetornaUsuarioDto()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = Usuario.Crear(
            "testuser",
            "Test User",
            Correo.Crear("test@example.com"),
            Telefono.Crear("1234567890"),
            Direccion.Crear("Calle Test 123"),
            Rol.User);

        var query = new ConsultarUsuarioQuery
        {
            UsuarioId = usuarioId
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Username.Should().Be("testuser");
        resultado.Nombre.Should().Be("Test User");
        resultado.Correo.Should().Be("test@example.com");
        resultado.Telefono.Should().Be("1234567890");
        resultado.Direccion.Should().Be("Calle Test 123");
        resultado.Rol.Should().Be(Rol.User);
    }

    [Fact]
    public async Task Handle_ConUsuarioInexistente_RetornaNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new ConsultarUsuarioQuery
        {
            UsuarioId = usuarioId
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ConUsuarioInactivo_RetornaNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = Usuario.Crear(
            "testuser",
            "Test User",
            Correo.Crear("test@example.com"),
            Telefono.Crear("1234567890"),
            Direccion.Crear("Calle Test 123"),
            Rol.User);
        
        // Desactivar el usuario
        usuario.Desactivar();

        var query = new ConsultarUsuarioQuery
        {
            UsuarioId = usuarioId
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DtoContieneTodosLosDatosDelUsuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = Usuario.Crear(
            "testuser",
            "Test User",
            Correo.Crear("test@example.com"),
            Telefono.Crear("1234567890"),
            Direccion.Crear("Calle Test 123"),
            Rol.Admin);

        var query = new ConsultarUsuarioQuery
        {
            UsuarioId = usuarioId
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(usuario.Id);
        resultado.Username.Should().Be(usuario.Username);
        resultado.Nombre.Should().Be(usuario.Nombre);
        resultado.Correo.Should().Be(usuario.Correo.Valor);
        resultado.Telefono.Should().Be(usuario.Telefono.Valor);
        resultado.Direccion.Should().Be(usuario.Direccion.Valor);
        resultado.Rol.Should().Be(usuario.Rol);
        resultado.FechaCreacion.Should().Be(usuario.FechaCreacion);
    }
}
