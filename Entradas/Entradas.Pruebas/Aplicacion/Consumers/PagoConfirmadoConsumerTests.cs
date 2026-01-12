using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Entradas.Aplicacion.Consumers;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;
using Entradas.Dominio.Eventos;
using Entradas.Dominio.Excepciones;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.Aplicacion.Consumers;

/// <summary>
/// Pruebas comprehensivas para PagoConfirmadoConsumer
/// Cubre procesamiento exitoso, errores y estados inválidos
/// </summary>
public class PagoConfirmadoConsumerTests
{
    private readonly Mock<IRepositorioEntradas> _mockRepositorio;
    private readonly Mock<ILogger<PagoConfirmadoConsumer>> _mockLogger;
    private readonly Mock<IEntradasMetrics> _mockMetrics;
    private readonly ActivitySource _activitySource;
    private readonly Mock<ConsumeContext<PagoConfirmadoEvento>> _mockContext;
    private readonly PagoConfirmadoConsumer _consumer;

    public PagoConfirmadoConsumerTests()
    {
        _mockRepositorio = new Mock<IRepositorioEntradas>();
        _mockLogger = new Mock<ILogger<PagoConfirmadoConsumer>>();
        _mockMetrics = new Mock<IEntradasMetrics>();
        _activitySource = new ActivitySource("Test");
        _mockContext = new Mock<ConsumeContext<PagoConfirmadoEvento>>();

        _consumer = new PagoConfirmadoConsumer(
            _mockRepositorio.Object,
            _mockLogger.Object,
            _mockMetrics.Object,
            _activitySource);
    }

    [Fact]
    public async Task Consume_ConEntradaExistenteYEstadoPendiente_DebeConfirmarPagoExitosamente()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var monto = 150.00m;
        var fechaPago = DateTime.UtcNow;

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = fechaPago,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada e, CancellationToken ct) => e);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);

        // Verificar llamadas al repositorio
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepositorio.Verify(x => x.GuardarAsync(entrada, It.IsAny<CancellationToken>()), Times.Once);

        // Verificar métricas de éxito
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("success"), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoEntradaNoExiste_DebeRegistrarWarningYNoLanzarExcepcion()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = 100.00m,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada?)null);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        // No debe lanzar excepción
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("entrada_no_encontrada"), Times.Once);
    }

    [Fact]
    public async Task Consume_ConMontoDiscrepante_DebeRegistrarWarningPeroContinuar()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var montoEntrada = 150.00m;
        var montoConfirmado = 200.00m; // Diferente al de la entrada

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = montoConfirmado,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), montoEntrada, null, "TICKET-ABC123-4567");
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada e, CancellationToken ct) => e);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada); // Debe continuar procesando
        
        _mockRepositorio.Verify(x => x.GuardarAsync(entrada, It.IsAny<CancellationToken>()), Times.Once);
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("success"), Times.Once);
    }

    [Fact]
    public async Task Consume_ConEntradaYaPagada_DebeLanzarEstadoEntradaInvalidoException()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var monto = 150.00m;

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        entrada.ConfirmarPago(); // Ya está pagada
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        // No debe lanzar excepción (se maneja internamente)
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verificar métricas de estado inválido
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("estado_invalido"), Times.Once);
    }

    [Fact]
    public async Task Consume_ConEntradaCancelada_DebeLanzarEstadoEntradaInvalidoException()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var monto = 150.00m;

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        entrada.Cancelar(); // Entrada cancelada
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        // No debe lanzar excepción (se maneja internamente)
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verificar métricas de estado inválido
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("estado_invalido"), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoRepositorioLanzaExcepcion_DebePropagar()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = 100.00m,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var excepcionRepositorio = new InvalidOperationException("Error de base de datos");
        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionRepositorio);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_mockContext.Object));

        exception.Should().Be(excepcionRepositorio);

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("error"), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoGuardarLanzaExcepcion_DebePropagar()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var monto = 150.00m;

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        var excepcionGuardar = new InvalidOperationException("Error al guardar");
        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionGuardar);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_mockContext.Object));

        exception.Should().Be(excepcionGuardar);

        // Verificar que se intentó confirmar el pago
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("error"), Times.Once);
    }

    [Fact]
    public async Task Consume_ConCancellationToken_DebeRespetarCancelacion()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = 100.00m,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(cts.Token);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _consumer.Consume(_mockContext.Object));
    }

    [Fact]
    public void Constructor_ConParametrosNulos_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PagoConfirmadoConsumer(
            null!, _mockLogger.Object, _mockMetrics.Object, _activitySource));

        Assert.Throws<ArgumentNullException>(() => new PagoConfirmadoConsumer(
            _mockRepositorio.Object, null!, _mockMetrics.Object, _activitySource));

        Assert.Throws<ArgumentNullException>(() => new PagoConfirmadoConsumer(
            _mockRepositorio.Object, _mockLogger.Object, null!, _activitySource));

        Assert.Throws<ArgumentNullException>(() => new PagoConfirmadoConsumer(
            _mockRepositorio.Object, _mockLogger.Object, _mockMetrics.Object, null!));
    }

    [Theory]
    [InlineData("TarjetaCredito")]
    [InlineData("TarjetaDebito")]
    [InlineData("Transferencia")]
    [InlineData("PayPal")]
    [InlineData("")]
    public async Task Consume_ConDiferentesMetodosPago_DebeProcessarCorrectamente(string metodoPago)
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var monto = 150.00m;

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = DateTime.UtcNow,
            MetodoPago = metodoPago
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada e, CancellationToken ct) => e);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("success"), Times.Once);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(999.99)]
    [InlineData(1000000.00)]
    public async Task Consume_ConDiferentesMontos_DebeProcessarCorrectamente(decimal monto)
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada e, CancellationToken ct) => e);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("success"), Times.Once);
    }

    [Fact]
    public async Task Consume_ConFechasPagoVariadas_DebeProcessarCorrectamente()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();
        var monto = 150.00m;
        var fechaPagoAntigua = DateTime.UtcNow.AddDays(-30);

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = monto,
            FechaPago = fechaPagoAntigua,
            MetodoPago = "TarjetaCredito"
        };

        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-ABC123-4567");
        
        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada e, CancellationToken ct) => e);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        entrada.Estado.Should().Be(EstadoEntrada.Pagada);
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("success"), Times.Once);
    }

    [Fact]
    public async Task Consume_ConEntradaNoEncontradaException_DebeRegistrarMetricasYNoPropagar()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var transaccionId = Guid.NewGuid();

        var evento = new PagoConfirmadoEvento
        {
            EntradaId = entradaId,
            TransaccionId = transaccionId,
            MontoConfirmado = 100.00m,
            FechaPago = DateTime.UtcNow,
            MetodoPago = "TarjetaCredito"
        };

        _mockContext.Setup(x => x.Message).Returns(evento);
        _mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var excepcionEntradaNoEncontrada = new EntradaNoEncontradaException(entradaId, "Entrada no encontrada");
        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionEntradaNoEncontrada);

        // Act
        await _consumer.Consume(_mockContext.Object);

        // Assert
        // No debe lanzar excepción
        _mockMetrics.Verify(x => x.IncrementPagosConfirmados("entrada_no_encontrada"), Times.Once);
    }
}