using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notificaciones.Aplicacion.Consumers;
using Notificaciones.Dominio.ContratosExternos;
using Notificaciones.Dominio.Interfaces;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.Aplicacion.Consumers;

public class CuponEnviadoConsumerTests
{
    private readonly Mock<IServicioEmail> _servicioEmailMock;
    private readonly Mock<INotificadorRealTime> _notificadorMock;
    private readonly Mock<IServicioUsuarios> _servicioUsuariosMock;
    private readonly Mock<ILogger<CuponEnviadoConsumer>> _loggerMock;
    private readonly CuponEnviadoConsumer _consumer;

    public CuponEnviadoConsumerTests()
    {
        _servicioEmailMock = new Mock<IServicioEmail>();
        _notificadorMock = new Mock<INotificadorRealTime>();
        _servicioUsuariosMock = new Mock<IServicioUsuarios>();
        _loggerMock = new Mock<ILogger<CuponEnviadoConsumer>>();

        _consumer = new CuponEnviadoConsumer(
            _servicioEmailMock.Object,
            _notificadorMock.Object,
            _servicioUsuariosMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_CuandoEventoEsRecibido_DebeEnviarEmailYPush()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = "usuario@test.com";
        var evento = new CuponEnviadoEvento(
            UsuarioDestinatarioId: usuarioId,
            Codigo: "REGLO100",
            Valor: 100,
            TipoDescuento: "MontoFijo",
            FechaExpiracion: DateTime.UtcNow.AddDays(7)
        );

        var contextMock = new Mock<ConsumeContext<CuponEnviadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(evento);

        _servicioUsuariosMock.Setup(s => s.ObtenerEmailUsuarioAsync(usuarioId))
            .ReturnsAsync(email);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        // 1. Verificar Email
        _servicioEmailMock.Verify(e => e.EnviarEmailAsync(
            email, 
            It.Is<string>(s => s.Contains("Regalo")), 
            It.Is<string>(s => s.Contains("REGLO100"))), 
            Times.Once);

        // 2. Verificar SignalR
        _notificadorMock.Verify(n => n.EnviarMensajeUsuarioAsync(
            usuarioId.ToString(), 
            It.Is<string>(s => s.Contains("REGLO100"))), 
            Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoNoSeEncuentraEmail_NoDebeEnviarEmail()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var evento = new CuponEnviadoEvento(usuarioId, "ERROR", 0, "P", DateTime.UtcNow);
        
        var contextMock = new Mock<ConsumeContext<CuponEnviadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(evento);

        _servicioUsuariosMock.Setup(s => s.ObtenerEmailUsuarioAsync(usuarioId))
            .ReturnsAsync((string?)null);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _servicioEmailMock.Verify(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
