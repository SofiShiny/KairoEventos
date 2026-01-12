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
/// Pruebas comprehensivas para ObtenerEntradasPorUsuarioQueryHandler
/// </summary>
public class ObtenerEntradasPorUsuarioQueryHandlerTests
{
    private readonly Mock<IRepositorioEntradas> _mockRepositorio;
    private readonly Mock<ILogger<ObtenerEntradasPorUsuarioQueryHandler>> _mockLogger;
    private readonly ObtenerEntradasPorUsuarioQueryHandler _handler;

    public ObtenerEntradasPorUsuarioQueryHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioEntradas>();
        _mockLogger = new Mock<ILogger<ObtenerEntradasPorUsuarioQueryHandler>>();
        _handler = new ObtenerEntradasPorUsuarioQueryHandler(_mockRepositorio.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CuandoUsuarioTieneEntradas_DebeRetornarListaDeEntradas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId1 = Guid.NewGuid();
        var eventoId2 = Guid.NewGuid();
        var asientoId1 = Guid.NewGuid();

        var entrada1 = Entrada.Crear(eventoId1, usuarioId, 100.00m, asientoId1, "TICKET-ABC123-4567");
        var entrada2 = Entrada.Crear(eventoId2, usuarioId, 75.50m, null, "TICKET-DEF456-7890");
        entrada2.ConfirmarPago(); // Una entrada pagada

        var entradas = new List<Entrada> { entrada1, entrada2 };
        
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultadoLista = resultado.ToList();
        resultadoLista.Should().HaveCount(2);

        var dto1 = resultadoLista.First(x => x.Id == entrada1.Id);
        dto1.EventoId.Should().Be(eventoId1);
        dto1.UsuarioId.Should().Be(usuarioId);
        dto1.AsientoId.Should().Be(asientoId1);
        dto1.Monto.Should().Be(100.00m);
        dto1.Estado.Should().Be(EstadoEntrada.PendientePago);

        var dto2 = resultadoLista.First(x => x.Id == entrada2.Id);
        dto2.EventoId.Should().Be(eventoId2);
        dto2.UsuarioId.Should().Be(usuarioId);
        dto2.AsientoId.Should().BeNull();
        dto2.Monto.Should().Be(75.50m);
        dto2.Estado.Should().Be(EstadoEntrada.Pagada);

        // Verificar que se llamó al repositorio
        _mockRepositorio.Verify(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoUsuarioNoTieneEntradas_DebeRetornarListaVacia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Entrada>());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();

        // Verificar que se llamó al repositorio
        _mockRepositorio.Verify(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEntradasEnDiferentesEstados_DebeRetornarTodas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        var entradaPendiente = Entrada.Crear(eventoId, usuarioId, 100.00m, null, "TICKET-ABC123-4567");
        
        var entradaPagada = Entrada.Crear(eventoId, usuarioId, 100.00m, null, "TICKET-DEF456-7890");
        entradaPagada.ConfirmarPago();
        
        var entradaCancelada = Entrada.Crear(eventoId, usuarioId, 100.00m, null, "TICKET-GHI789-0123");
        entradaCancelada.Cancelar();
        
        var entradaUsada = Entrada.Crear(eventoId, usuarioId, 100.00m, null, "TICKET-JKL012-3456");
        entradaUsada.ConfirmarPago();
        entradaUsada.MarcarComoUsada();

        var entradas = new List<Entrada> { entradaPendiente, entradaPagada, entradaCancelada, entradaUsada };
        
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultadoLista = resultado.ToList();
        resultadoLista.Should().HaveCount(4);

        resultadoLista.Should().Contain(x => x.Estado == EstadoEntrada.PendientePago);
        resultadoLista.Should().Contain(x => x.Estado == EstadoEntrada.Pagada);
        resultadoLista.Should().Contain(x => x.Estado == EstadoEntrada.Cancelada);
        resultadoLista.Should().Contain(x => x.Estado == EstadoEntrada.Usada);
    }

    [Fact]
    public async Task Handle_ConMuchasEntradas_DebeRetornarTodasCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradas = new List<Entrada>();

        // Crear 10 entradas para el usuario
        for (int i = 0; i < 10; i++)
        {
            var eventoId = Guid.NewGuid();
            var monto = 50.00m + (i * 10);
            var codigoQr = $"TICKET-{i:D3}123-4567";
            var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
            entradas.Add(entrada);
        }
        
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultadoLista = resultado.ToList();
        resultadoLista.Should().HaveCount(10);

        // Verificar que todos los DTOs tienen el mismo usuario
        resultadoLista.Should().OnlyContain(x => x.UsuarioId == usuarioId);

        // Verificar que los montos son correctos
        for (int i = 0; i < 10; i++)
        {
            var expectedMonto = 50.00m + (i * 10);
            resultadoLista.Should().Contain(x => x.Monto == expectedMonto);
        }
    }

    [Fact]
    public async Task Handle_CuandoRepositorioLanzaExcepcion_DebePropagar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        var excepcionRepositorio = new InvalidOperationException("Error de base de datos");
        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
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
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public void Constructor_ConParametrosNulos_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ObtenerEntradasPorUsuarioQueryHandler(null!, _mockLogger.Object));
        Assert.Throws<ArgumentNullException>(() => new ObtenerEntradasPorUsuarioQueryHandler(_mockRepositorio.Object, null!));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_ConGuidVacio_DebeConsultarRepositorio(string guidString)
    {
        // Arrange
        var usuarioId = Guid.Parse(guidString);
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Entrada>());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeEmpty();
        _mockRepositorio.Verify(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEntradasConAsientosYSinAsientos_DebeRetornarAmbasTipos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        var entradaConAsiento = Entrada.Crear(eventoId, usuarioId, 150.00m, asientoId, "TICKET-ABC123-4567");
        var entradaGeneral = Entrada.Crear(eventoId, usuarioId, 100.00m, null, "TICKET-DEF456-7890");

        var entradas = new List<Entrada> { entradaConAsiento, entradaGeneral };
        
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultadoLista = resultado.ToList();
        resultadoLista.Should().HaveCount(2);

        var dtoConAsiento = resultadoLista.First(x => x.AsientoId.HasValue);
        dtoConAsiento.AsientoId.Should().Be(asientoId);
        dtoConAsiento.Monto.Should().Be(150.00m);

        var dtoGeneral = resultadoLista.First(x => !x.AsientoId.HasValue);
        dtoGeneral.AsientoId.Should().BeNull();
        dtoGeneral.Monto.Should().Be(100.00m);
    }

    [Fact]
    public async Task Handle_ConEntradasDeDiferentesEventos_DebeRetornarTodas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId1 = Guid.NewGuid();
        var eventoId2 = Guid.NewGuid();
        var eventoId3 = Guid.NewGuid();

        var entrada1 = Entrada.Crear(eventoId1, usuarioId, 100.00m, null, "TICKET-ABC123-4567");
        var entrada2 = Entrada.Crear(eventoId2, usuarioId, 200.00m, null, "TICKET-DEF456-7890");
        var entrada3 = Entrada.Crear(eventoId3, usuarioId, 300.00m, null, "TICKET-GHI789-0123");

        var entradas = new List<Entrada> { entrada1, entrada2, entrada3 };
        
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultadoLista = resultado.ToList();
        resultadoLista.Should().HaveCount(3);

        resultadoLista.Should().Contain(x => x.EventoId == eventoId1);
        resultadoLista.Should().Contain(x => x.EventoId == eventoId2);
        resultadoLista.Should().Contain(x => x.EventoId == eventoId3);

        // Todos deben tener el mismo usuario
        resultadoLista.Should().OnlyContain(x => x.UsuarioId == usuarioId);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(999.99)]
    [InlineData(1000000.00)]
    public async Task Handle_ConDiferentesMontos_DebeRetornarMontosCorrectamente(decimal monto)
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var codigoQr = "TICKET-PQR678-9012";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        var entradas = new List<Entrada> { entrada };
        
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultadoLista = resultado.ToList();
        resultadoLista.Should().HaveCount(1);
        resultadoLista.First().Monto.Should().Be(monto);
    }
}