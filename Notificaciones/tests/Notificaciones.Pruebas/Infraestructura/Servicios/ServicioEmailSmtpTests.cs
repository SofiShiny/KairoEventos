using Moq;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notificaciones.Infraestructura.Servicios;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.Infraestructura.Servicios;

public class ServicioEmailSmtpTests
{
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly Mock<ILogger<ServicioEmailSmtp>> _loggerMock;
    private readonly IOptions<ConfiguracionEmail> _options;

    public ServicioEmailSmtpTests()
    {
        _smtpClientMock = new Mock<ISmtpClient>();
        _loggerMock = new Mock<ILogger<ServicioEmailSmtp>>();
        _options = Options.Create(new ConfiguracionEmail
        {
            Host = "smtp.test.com",
            Puerto = 587,
            Usuario = "user@test.com",
            Password = "password",
            NombreEmisor = "Test Sender",
            EmailEmisor = "sender@test.com",
            UsarSsl = false
        });
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoTodoEsCorrecto_DebeConectarYEnviar()
    {
        // Arrange
        var servicio = new ServicioEmailSmtp(_options, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Test Subject", "<p>content</p>");

        // Assert
        _smtpClientMock.Verify(c => c.ConnectAsync("smtp.test.com", 587, SecureSocketOptions.StartTls, default), Times.Once);
        _smtpClientMock.Verify(c => c.AuthenticateAsync("user@test.com", "password", default), Times.Once);
        _smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), default, null), Times.Once);
        _smtpClientMock.Verify(c => c.DisconnectAsync(true, default), Times.Once);
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoNoHayUsuario_NoDebeAutenticar()
    {
        // Arrange
        var optionsNoAuth = Options.Create(new ConfiguracionEmail
        {
            Host = "smtp.test.com",
            Puerto = 587,
            Usuario = "", // Vacío
            Password = "",
            NombreEmisor = "Test",
            EmailEmisor = "test@test.com",
            UsarSsl = false
        });
        var servicio = new ServicioEmailSmtp(optionsNoAuth, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Subject", "body");

        // Assert
        _smtpClientMock.Verify(c => c.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoUsaSsl_DebeConectarConSsl()
    {
        // Arrange
        var optionsSsl = Options.Create(new ConfiguracionEmail
        {
            Host = "smtp.test.com",
            Puerto = 465,
            Usuario = "user",
            Password = "pass",
            NombreEmisor = "Test",
            EmailEmisor = "test@test.com",
            UsarSsl = true // SSL true
        });
        var servicio = new ServicioEmailSmtp(optionsSsl, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Subject", "body");

        // Assert
        _smtpClientMock.Verify(c => c.ConnectAsync("smtp.test.com", 465, SecureSocketOptions.SslOnConnect, default), Times.Once);
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoFallaConexion_DebeLoguearError()
    {
        // Arrange
        _smtpClientMock.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), default))
            .ThrowsAsync(new System.Exception("Connection failed"));

        var servicio = new ServicioEmailSmtp(_options, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Test Subject", "<p>content</p>");

        // Assert
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error crítico al enviar email")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void ConfiguracionEmail_Properties_ShouldWork()
    {
        // Test para cubrir la clase de configuración (7 líneas)
        var config = new ConfiguracionEmail();
        config.Host = "test";
        config.Puerto = 123;
        config.Usuario = "user";
        config.Password = "pass";
        config.NombreEmisor = "name";
        config.EmailEmisor = "email";
        config.UsarSsl = true;

        config.Host.Should().Be("test");
        config.Puerto.Should().Be(123);
        config.Usuario.Should().Be("user");
        config.Password.Should().Be("pass");
        config.NombreEmisor.Should().Be("name");
        config.EmailEmisor.Should().Be("email");
        config.UsarSsl.Should().BeTrue();
    }

    [Fact]
    public void Constructor_SinFactory_DebeUsarDefault()
    {
        // Test para cubrir la rama '??' del constructor
        var servicio = new ServicioEmailSmtp(_options, _loggerMock.Object, null);
        servicio.Should().NotBeNull();
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoFallaEnvio_DebeLoguearError()
    {
        // Arrange
        _smtpClientMock.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), default, null))
            .ThrowsAsync(new System.Exception("Send failed"));

        var servicio = new ServicioEmailSmtp(_options, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Subject", "body");

        // Assert
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error crítico al enviar email")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoFallaAutenticacion_DebeLoguearError()
    {
        // Arrange
        _smtpClientMock.Setup(c => c.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ThrowsAsync(new System.Exception("Auth failed"));

        var servicio = new ServicioEmailSmtp(_options, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Subject", "body");

        // Assert
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error crítico")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task EnviarEmailAsync_CuandoFallaDisconnect_DebeLoguearError()
    {
        // Arrange
        _smtpClientMock.Setup(c => c.DisconnectAsync(true, default))
            .ThrowsAsync(new System.Exception("Disconnect failed"));

        var servicio = new ServicioEmailSmtp(_options, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Subject", "body");

        // Assert - Debe loguear el error del disconnect
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error crítico")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task EnviarEmailAsync_ConUsuarioVacio_NoDebeAutenticar()
    {
        // Arrange - Configuración sin usuario
        var optionsNoUser = Options.Create(new ConfiguracionEmail
        {
            Host = "smtp.test.com",
            Puerto = 587,
            Usuario = string.Empty, // Usuario vacío
            Password = "",
            NombreEmisor = "Test",
            EmailEmisor = "test@test.com",
            UsarSsl = false
        });
        var servicio = new ServicioEmailSmtp(optionsNoUser, _loggerMock.Object, () => _smtpClientMock.Object);

        // Act
        await servicio.EnviarEmailAsync("dest@test.com", "Subject", "body");

        // Assert - No debe llamar a AuthenticateAsync
        _smtpClientMock.Verify(c => c.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        // Pero sí debe enviar el email
        _smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), default, null), Times.Once);
    }
}
