using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Entradas.Aplicacion.Handlers;
using Entradas.Aplicacion.Queries;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.Aplicacion.Handlers;

/// <summary>
/// Pruebas comprehensivas para ObtenerEntradaQueryHandler
/// </summary>
public class ObtenerEntradaQueryHandlerTests
{
    private readonly Mock<IRepositorioEntradas> _mockRepositorio;
    private readonly Mock<ILogger<ObtenerEntradaQueryHandler>> _mockLogger;
    private readonly ObtenerEntradaQueryHandler _handler;

    public ObtenerEntradaQueryHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioEntradas>();
        _mockLogger = new Mock<ILogger<ObtenerEntradaQueryHandler>>();
        _handler = new ObtenerEntradaQueryHandler(_mockRepositorio.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CuandoEntradaExiste_DebeRetornarEntradaDto()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 150.00m;
        var codigoQr = "TICKET-ABC123-4567";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, asientoId, codigoQr);
        
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(entrada.Id);
        resultado.EventoId.Should().Be(eventoId);
        resultado.UsuarioId.Should().Be(usuarioId);
        resultado.AsientoId.Should().Be(asientoId);
        resultado.Monto.Should().Be(monto);
        resultado.CodigoQr.Should().Be(codigoQr);
        resultado.Estado.Should().Be(EstadoEntrada.PendientePago);
        resultado.FechaCompra.Should().Be(entrada.FechaCompra);

        // Verificar que se llamó al repositorio
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoEntradaNoExiste_DebeRetornarNull()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada?)null);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeNull();

        // Verificar que se llamó al repositorio
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEntradaGeneral_DebeRetornarDtoConAsientoIdNull()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 75.50m;
        var codigoQr = "TICKET-DEF456-7890";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.AsientoId.Should().BeNull();
        resultado.EventoId.Should().Be(eventoId);
        resultado.UsuarioId.Should().Be(usuarioId);
        resultado.Monto.Should().Be(monto);
        resultado.CodigoQr.Should().Be(codigoQr);
    }

    [Fact]
    public async Task Handle_ConEntradaPagada_DebeRetornarEstadoCorrecto()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 200.00m;
        var codigoQr = "TICKET-GHI789-0123";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        entrada.ConfirmarPago(); // Cambiar estado a Pagada
        
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Estado.Should().Be(EstadoEntrada.Pagada);
    }

    [Fact]
    public async Task Handle_CuandoRepositorioLanzaExcepcion_DebePropagar()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var query = new ObtenerEntradaQuery(entradaId);

        var excepcionRepositorio = new InvalidOperationException("Error de base de datos");
        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionRepositorio);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(excepcionRepositorio);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DebeRespetarCancelacion()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var query = new ObtenerEntradaQuery(entradaId);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public void Constructor_ConParametrosNulos_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ObtenerEntradaQueryHandler(null!, _mockLogger.Object));
        Assert.Throws<ArgumentNullException>(() => new ObtenerEntradaQueryHandler(_mockRepositorio.Object, null!));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_ConGuidVacio_DebeConsultarRepositorio(string guidString)
    {
        // Arrange
        var entradaId = Guid.Parse(guidString);
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entrada?)null);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeNull();
        _mockRepositorio.Verify(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEntradaCancelada_DebeRetornarEstadoCorrecto()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;
        var codigoQr = "TICKET-JKL012-3456";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        entrada.Cancelar(); // Cambiar estado a Cancelada
        
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Estado.Should().Be(EstadoEntrada.Cancelada);
    }

    [Fact]
    public async Task Handle_ConEntradaUsada_DebeRetornarEstadoCorrecto()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 300.00m;
        var codigoQr = "TICKET-MNO345-6789";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        entrada.ConfirmarPago(); // Primero pagar
        entrada.MarcarComoUsada(); // Luego usar
        
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Estado.Should().Be(EstadoEntrada.Usada);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(999.99)]
    [InlineData(1000000.00)]
    public async Task Handle_ConDiferentesMontos_DebeRetornarMontoCorrectamente(decimal monto)
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var codigoQr = "TICKET-PQR678-9012";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        
        var query = new ObtenerEntradaQuery(entradaId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorIdAsync(entradaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entrada);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Monto.Should().Be(monto);
    }
}