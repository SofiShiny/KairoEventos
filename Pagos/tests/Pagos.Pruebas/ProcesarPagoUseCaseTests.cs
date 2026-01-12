using Moq;
using Pagos.Aplicacion.CasosUso;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;
using Pagos.Aplicacion.Eventos;
using MassTransit;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;

namespace Pagos.Pruebas;

public class ProcesarPagoUseCaseTests
{
    private readonly Mock<IRepositorioTransacciones> _repoMock = new();
    private readonly Mock<IPasarelaPago> _pasarelaMock = new();
    private readonly Mock<IGeneradorFactura> _facturaMock = new();
    private readonly Mock<IAlmacenadorArchivos> _almacenadorMock = new();
    private readonly Mock<IPublishEndpoint> _publishMock = new();
    private readonly Mock<ILogger<ProcesarPagoUseCase>> _loggerMock = new();
    private readonly ProcesarPagoUseCase _sut;

    public ProcesarPagoUseCaseTests()
    {
        _sut = new ProcesarPagoUseCase(
            _repoMock.Object, _pasarelaMock.Object, _facturaMock.Object,
            _almacenadorMock.Object, _publishMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoPagoEsExitoso_DebeAprobarYGenerarFactura()
    {
        // Arrange
        var txId = Guid.NewGuid();
        var tx = new Transaccion { Id = txId, Monto = 100 };
        _repoMock.Setup(r => r.ObtenerPorIdAsync(txId)).ReturnsAsync(tx);
        _pasarelaMock.Setup(p => p.CobrarAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new ResultadoPago(true, null, "EXT-123"));
        _facturaMock.Setup(f => f.GenerarPdf(tx)).Returns(new byte[] { 1, 2, 3 });
        _almacenadorMock.Setup(a => a.GuardarAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
            .ReturnsAsync("url-factura");

        // Act
        await _sut.EjecutarAsync(txId, "123456780000");

        // Assert
        tx.Estado.Should().Be(Pagos.Dominio.Modelos.EstadoTransaccion.Aprobado);
        _publishMock.Verify(p => p.Publish(It.IsAny<PagoAprobadoEvento>(), default), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoPagoEsRechazado_DebeMarcarComoRechazado()
    {
        // Arrange
        var txId = Guid.NewGuid();
        var tx = new Transaccion { Id = txId, Monto = 100 };
        _repoMock.Setup(r => r.ObtenerPorIdAsync(txId)).ReturnsAsync(tx);
        _pasarelaMock.Setup(p => p.CobrarAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new ResultadoPago(false, "Fondos insuficientes", null));

        // Act
        await _sut.EjecutarAsync(txId, "123456789999");

        // Assert
        tx.Estado.Should().Be(Pagos.Dominio.Modelos.EstadoTransaccion.Rechazado);
        tx.MensajeError.Should().Be("Fondos insuficientes");
        _publishMock.Verify(p => p.Publish(It.IsAny<PagoRechazadoEvento>(), default), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoPasarelaLanzaExcepcion_DebeSubirExcepcionParaHangfire()
    {
        // Arrange
        var txId = Guid.NewGuid();
        var tx = new Transaccion { Id = txId, Monto = 100 };
        _repoMock.Setup(r => r.ObtenerPorIdAsync(txId)).ReturnsAsync(tx);
        _pasarelaMock.Setup(p => p.CobrarAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Red caída"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.EjecutarAsync(txId, "123456785000"));
    }

    [Fact]
    public async Task EjecutarAsync_CuandoTransaccionNoExiste_DebeRetornarSinHacerNada()
    {
        // Arrange
        var txId = Guid.NewGuid();
        _repoMock.Setup(r => r.ObtenerPorIdAsync(txId)).ReturnsAsync((Transaccion?)null);

        // Act
        await _sut.EjecutarAsync(txId, "1234");

        // Assert
        _pasarelaMock.Verify(p => p.CobrarAsync(It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }
}
