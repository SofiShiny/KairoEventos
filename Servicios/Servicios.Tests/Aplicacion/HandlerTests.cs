using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MassTransit;
using Servicios.Aplicacion.Comandos;
using Servicios.Dominio.Entidades;
using Servicios.Dominio.Repositorios;
using Servicios.Aplicacion.Eventos;
using Servicios.Aplicacion.Consumers;

namespace Servicios.Tests.Aplicacion;

public class ReservarServicioHandlerTests
{
    private readonly Mock<IRepositorioServicios> _repositorioMock;
    private readonly Mock<IVerificadorEntradas> _verificadorMock;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly Mock<ILogger<ReservarServicioCommandHandler>> _loggerMock;
    private readonly ReservarServicioCommandHandler _handler;

    public ReservarServicioHandlerTests()
    {
        _repositorioMock = new Mock<IRepositorioServicios>();
        _verificadorMock = new Mock<IVerificadorEntradas>();
        _publishMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<ReservarServicioCommandHandler>>();
        
        _handler = new ReservarServicioCommandHandler(
            _repositorioMock.Object,
            _verificadorMock.Object,
            _publishMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ReservarServicio_ConEntradaValida_DebeCrearReservaYPublicarEvento()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var servicioId = Guid.NewGuid();
        var servicio = new ServicioGlobal(servicioId, "Transporte VIP", 50.0m);
        
        var command = new ReservarServicioCommand(usuarioId, eventoId, servicioId);

        _repositorioMock.Setup(r => r.ObtenerServicioPorIdAsync(servicioId))
            .ReturnsAsync(servicio);
        
        _verificadorMock.Setup(v => v.UsuarioTieneEntradaParaEventoAsync(usuarioId, eventoId))
            .ReturnsAsync(true);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();
        _repositorioMock.Verify(r => r.AgregarReservaAsync(It.Is<ReservaServicio>(res => 
            res.UsuarioId == usuarioId && 
            res.EventoId == eventoId && 
            res.Estado == EstadoReserva.PendientePago)), Times.Once);

        _publishMock.Verify(p => p.Publish(It.IsAny<SolicitudPagoServicioCreada>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReservarServicio_SinEntrada_DebeLanzarExcepcion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var servicioId = Guid.NewGuid();
        var servicio = new ServicioGlobal(servicioId, "Transporte VIP", 50.0m);
        
        var command = new ReservarServicioCommand(usuarioId, eventoId, servicioId);

        _repositorioMock.Setup(r => r.ObtenerServicioPorIdAsync(servicioId))
            .ReturnsAsync(servicio);
        
        _verificadorMock.Setup(v => v.UsuarioTieneEntradaParaEventoAsync(usuarioId, eventoId))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repositorioMock.Verify(r => r.AgregarReservaAsync(It.IsAny<ReservaServicio>()), Times.Never);
        _publishMock.Verify(p => p.Publish(It.IsAny<SolicitudPagoServicioCreada>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
 public class PagoAprobadoConsumerTests
 {
    private readonly Mock<IRepositorioServicios> _repositorioMock;
    private readonly Mock<ILogger<PagoAprobadoConsumer>> _loggerMock;
    private readonly PagoAprobadoConsumer _consumer;

    public PagoAprobadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioServicios>();
        _loggerMock = new Mock<ILogger<PagoAprobadoConsumer>>();
        _consumer = new PagoAprobadoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PagoAprobado_DebeConfirmarReserva()
    {
        // Arrange
        var reservaId = Guid.NewGuid();
        var reserva = new ReservaServicio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        
        var contextMock = new Mock<ConsumeContext<PagoAprobadoEvento>>();
        contextMock.Setup(c => c.Message).Returns(new PagoAprobadoEvento(
            Guid.NewGuid(), reservaId, Guid.NewGuid(), 50.0m, "http://factura.com"
        ));

        _repositorioMock.Setup(r => r.ObtenerReservaPorIdAsync(reservaId))
            .ReturnsAsync(reserva);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        reserva.Estado.Should().Be(EstadoReserva.Confirmado);
        _repositorioMock.Verify(r => r.ActualizarReservaAsync(reserva), Times.Once);
    }
 }
