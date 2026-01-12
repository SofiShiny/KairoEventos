using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Aplicacion.Comandos;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.Excepciones;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;

namespace Usuarios.Pruebas.Aplicacion;

public class ActualizarUsuarioComandoHandlerTests
{
    private readonly Mock<IRepositorioUsuarios> _mockRepositorio;
    private readonly Mock<IServicioKeycloak> _mockServicioKeycloak;
    private readonly Mock<ILogger<ActualizarUsuarioComandoHandler>> _mockLogger;
    private readonly ActualizarUsuarioComandoHandler _handler;

    public ActualizarUsuarioComandoHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioUsuarios>();
        _mockServicioKeycloak = new Mock<IServicioKeycloak>();
        _mockLogger = new Mock<ILogger<ActualizarUsuarioComandoHandler>>();
        
        _handler = new ActualizarUsuarioComandoHandler(
            _mockRepositorio.Object,
            _mockServicioKeycloak.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ConUsuarioExistente_RetornaUnit()
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

        var comando = new ActualizarUsuarioComando
        {
            UsuarioId = usuarioId,
            Nombre = "Updated Name",
            Telefono = "9876543210",
            Direccion = "Nueva Direccion 456"
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _mockServicioKeycloak
            .Setup(s => s.ActualizarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepositorio
            .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_ConUsuarioNoEncontrado_LanzaUsuarioNoEncontradoException()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comando = new ActualizarUsuarioComando
        {
            UsuarioId = usuarioId,
            Nombre = "Updated Name",
            Telefono = "9876543210",
            Direccion = "Nueva Direccion 456"
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioNoEncontradoException>(
            () => _handler.Handle(comando, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_LlamaUsuarioActualizar()
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

        var comando = new ActualizarUsuarioComando
        {
            UsuarioId = usuarioId,
            Nombre = "Updated Name",
            Telefono = "9876543210",
            Direccion = "Nueva Direccion 456"
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _mockServicioKeycloak
            .Setup(s => s.ActualizarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepositorio
            .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(comando, CancellationToken.None);

        // Assert
        usuario.Nombre.Should().Be(comando.Nombre);
        usuario.Telefono.Valor.Should().Be("9876543210");
        usuario.Direccion.Valor.Should().Be(comando.Direccion);
        usuario.FechaActualizacion.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_LlamaServicioKeycloakActualizarUsuarioAsync()
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

        var comando = new ActualizarUsuarioComando
        {
            UsuarioId = usuarioId,
            Nombre = "Updated Name",
            Telefono = "9876543210",
            Direccion = "Nueva Direccion 456"
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _mockServicioKeycloak
            .Setup(s => s.ActualizarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepositorio
            .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(comando, CancellationToken.None);

        // Assert
        _mockServicioKeycloak.Verify(
            s => s.ActualizarUsuarioAsync(
                It.IsAny<Usuario>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LlamaRepositorioActualizarAsync()
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

        var comando = new ActualizarUsuarioComando
        {
            UsuarioId = usuarioId,
            Nombre = "Updated Name",
            Telefono = "9876543210",
            Direccion = "Nueva Direccion 456"
        };

        _mockRepositorio
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _mockServicioKeycloak
            .Setup(s => s.ActualizarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepositorio
            .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(comando, CancellationToken.None);

        // Assert
        _mockRepositorio.Verify(
            r => r.ActualizarAsync(
                It.IsAny<Usuario>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
