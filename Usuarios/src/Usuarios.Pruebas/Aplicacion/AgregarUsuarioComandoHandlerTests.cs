using FluentAssertions;
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

public class AgregarUsuarioComandoHandlerTests
{
    private readonly Mock<IRepositorioUsuarios> _mockRepositorio;
    private readonly Mock<IServicioKeycloak> _mockServicioKeycloak;
    private readonly Mock<ILogger<AgregarUsuarioComandoHandler>> _mockLogger;
    private readonly AgregarUsuarioComandoHandler _handler;

    public AgregarUsuarioComandoHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioUsuarios>();
        _mockServicioKeycloak = new Mock<IServicioKeycloak>();
        _mockLogger = new Mock<ILogger<AgregarUsuarioComandoHandler>>();
        
        _handler = new AgregarUsuarioComandoHandler(
            _mockRepositorio.Object,
            _mockServicioKeycloak.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ConUsuarioValido_RetornaGuid()
    {
        // Arrange
        var comando = new AgregarUsuarioComando
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        _mockRepositorio
            .Setup(r => r.ExisteUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepositorio
            .Setup(r => r.ExisteCorreoAsync(It.IsAny<Correo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockServicioKeycloak
            .Setup(s => s.CrearUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("keycloak-id");

        _mockRepositorio
            .Setup(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ConUsernameDuplicado_LanzaUsernameDuplicadoException()
    {
        // Arrange
        var comando = new AgregarUsuarioComando
        {
            Username = "existinguser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        _mockRepositorio
            .Setup(r => r.ExisteUsernameAsync(comando.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<UsernameDuplicadoException>(
            () => _handler.Handle(comando, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ConCorreoDuplicado_LanzaCorreoDuplicadoException()
    {
        // Arrange
        var comando = new AgregarUsuarioComando
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "existing@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        _mockRepositorio
            .Setup(r => r.ExisteUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepositorio
            .Setup(r => r.ExisteCorreoAsync(It.IsAny<Correo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<CorreoDuplicadoException>(
            () => _handler.Handle(comando, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_LlamaServicioKeycloakCrearUsuarioAsync()
    {
        // Arrange
        var comando = new AgregarUsuarioComando
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        _mockRepositorio
            .Setup(r => r.ExisteUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepositorio
            .Setup(r => r.ExisteCorreoAsync(It.IsAny<Correo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockServicioKeycloak
            .Setup(s => s.CrearUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("keycloak-id");

        _mockRepositorio
            .Setup(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(comando, CancellationToken.None);

        // Assert
        _mockServicioKeycloak.Verify(
            s => s.CrearUsuarioAsync(
                It.IsAny<Usuario>(),
                comando.Password,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LlamaRepositorioAgregarAsync()
    {
        // Arrange
        var comando = new AgregarUsuarioComando
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        _mockRepositorio
            .Setup(r => r.ExisteUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepositorio
            .Setup(r => r.ExisteCorreoAsync(It.IsAny<Correo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockServicioKeycloak
            .Setup(s => s.CrearUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("keycloak-id");

        _mockRepositorio
            .Setup(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(comando, CancellationToken.None);

        // Assert
        _mockRepositorio.Verify(
            r => r.AgregarAsync(
                It.IsAny<Usuario>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RegistraLogDeOperacion()
    {
        // Arrange
        var comando = new AgregarUsuarioComando
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        _mockRepositorio
            .Setup(r => r.ExisteUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepositorio
            .Setup(r => r.ExisteCorreoAsync(It.IsAny<Correo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockServicioKeycloak
            .Setup(s => s.CrearUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("keycloak-id");

        _mockRepositorio
            .Setup(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(comando, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Agregando usuario")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usuario agregado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
