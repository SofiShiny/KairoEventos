using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using Entradas.Aplicacion.Consumers;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Interfaces;
using Pagos.Aplicacion.Eventos;
using Entradas.Dominio.Enums;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Aplicacion.Consumers;

public class PagoAprobadoConsumerTests
{
    private readonly Mock<IRepositorioEntradas> _repositorioMock;
    private readonly Mock<IGeneradorCodigoQr> _generadorQrMock;
    private readonly Mock<ILogger<PagoAprobadoConsumer>> _loggerMock;
    private readonly PagoAprobadoConsumer _consumer;

    public PagoAprobadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioEntradas>();
        _generadorQrMock = new Mock<IGeneradorCodigoQr>();
        _loggerMock = new Mock<ILogger<PagoAprobadoConsumer>>();
        _consumer = new PagoAprobadoConsumer(
            _repositorioMock.Object, 
            _generadorQrMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_CuandoEntradaExiste_DebeConfirmarPagoyGuardar()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var entrada = Entrada.Crear(eventoId, usuarioId, 100, null, "QR_TEMP");
        
        typeof(Entrada).GetProperty("Id")?.SetValue(entrada, entradaId);

        var mensaje = new PagoAprobadoEvento(
            TransaccionId: Guid.NewGuid(),
            OrdenId: entradaId,
            UsuarioId: usuarioId,
            Monto: 100,
            UrlFactura: "http://factura.pdf"
        );

        var contextMock = new Mock<ConsumeContext<PagoAprobadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(mensaje);
        contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _repositorioMock.Setup(r => r.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);
        
        _generadorQrMock.Setup(g => g.GenerarCodigoUnico()).Returns("QR_FINAL_123");

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);
        entrada.CodigoQr.Should().Be("QR_FINAL_123");
        _repositorioMock.Verify(r => r.GuardarAsync(It.Is<Entrada>(e => e.Id == entradaId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoEntradaNoExiste_DebeLoguearCritico()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var mensaje = new PagoAprobadoEvento(Guid.NewGuid(), entradaId, Guid.NewGuid(), 100, "url");

        var contextMock = new Mock<ConsumeContext<PagoAprobadoEvento>>();
        contextMock.Setup(x => x.Message).Returns(mensaje);
        contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _repositorioMock.Setup(r => r.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada?)null);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _repositorioMock.Verify(r => r.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(l => l.Log(
            LogLevel.Critical,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ERROR CRÍTICO")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
