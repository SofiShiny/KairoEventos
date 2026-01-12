using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Notificaciones.Aplicacion.Consumers;
using Notificaciones.Dominio.Contratos;
using Notificaciones.Dominio.Interfaces;
using Notificaciones.Infraestructura.Hubs;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.Aplicacion.Consumers;

public class PagoAprobadoConsumerTests
{
    private readonly Mock<INotificadorRealTime> _notificadorMock;
    private readonly Mock<IServicioEmail> _servicioEmailMock;
    private readonly Mock<IServicioUsuarios> _servicioUsuariosMock;
    private readonly Mock<ILogger<PagoAprobadoConsumer>> _loggerMock;
    private readonly PagoAprobadoConsumer _consumer;

    public PagoAprobadoConsumerTests()
    {
        _notificadorMock = new Mock<INotificadorRealTime>();
        _servicioEmailMock = new Mock<IServicioEmail>();
        _servicioUsuariosMock = new Mock<IServicioUsuarios>();
        _loggerMock = new Mock<ILogger<PagoAprobadoConsumer>>();

        // Configuración por defecto: Usuario encontrado con su correo
        _servicioUsuariosMock.Setup(s => s.ObtenerEmailUsuarioAsync(It.IsAny<Guid>()))
            .ReturnsAsync("usuario@test.com");

        _consumer = new PagoAprobadoConsumer(
            _notificadorMock.Object, 
            _servicioEmailMock.Object, 
            _servicioUsuariosMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_CuandoPagoEsExitoso_DebeEnviarNotificaciones()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var evento = new PagoAprobadoEvento(
            TransaccionId: Guid.NewGuid(),
            OrdenId: Guid.NewGuid(),
            UsuarioId: usuarioId,
            Monto: 150.50m,
            UrlFactura: "http://kairo.com/factura.pdf"
        );

        var contextMock = new Mock<ConsumeContext<PagoAprobadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(evento);
        contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        // 1. Verificar SignalR
        _notificadorMock.Verify(n => n.EnviarMensajeUsuarioAsync(
            usuarioId.ToString(), 
            It.Is<string>(s => s.Contains("150,50") || s.Contains("150.50"))), 
            Times.Once);

        // 2. Verificar Email
        _servicioEmailMock.Verify(e => e.EnviarEmailAsync(
            It.IsAny<string>(), 
            It.Is<string>(s => s.Contains("Confirmación de Pago")), 
            It.Is<string>(s => s.Contains(evento.UrlFactura))), 
            Times.Once);

        // 3. Verificar Logs
        _loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando notificaciones")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoEmailFalla_NoDebeLanzarExcepcion()
    {
        // Arrange
        var evento = new PagoAprobadoEvento(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, "url");
        var contextMock = new Mock<ConsumeContext<PagoAprobadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(evento);

        _servicioEmailMock.Setup(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("SMTP Error"));

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock.Object);

        // Assert
        // En nuestro diseño actual, el consumer NO lanza excepción si el email falla (catch interno con log)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_CuandoUsuarioNoTieneEmail_NoDebeEnviarEmail()
    {
        // Arrange
        var evento = new PagoAprobadoEvento(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, "url");
        var contextMock = new Mock<ConsumeContext<PagoAprobadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(evento);

        _servicioUsuariosMock.Setup(s => s.ObtenerEmailUsuarioAsync(It.IsAny<Guid>()))
            .ReturnsAsync((string?)null);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _servicioEmailMock.Verify(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no se encontró el correo")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
