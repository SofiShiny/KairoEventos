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
/// Pruebas comprehensivas para ObtenerHistorialUsuarioQueryHandler
/// Verifica el correcto funcionamiento del endpoint de historial de usuario
/// </summary>
public class ObtenerHistorialUsuarioQueryHandlerTests
{
    private readonly Mock<IRepositorioEntradas> _mockRepositorio;
    private readonly Mock<ILogger<ObtenerHistorialUsuarioQueryHandler>> _mockLogger;
    private readonly ObtenerHistorialUsuarioQueryHandler _handler;

    public ObtenerHistorialUsuarioQueryHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioEntradas>();
        _mockLogger = new Mock<ILogger<ObtenerHistorialUsuarioQueryHandler>>();
        _handler = new ObtenerHistorialUsuarioQueryHandler(_mockRepositorio.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CuandoUsuarioTieneEntradas_DebeRetornarHistorialOrdenadoPorFecha()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId1 = Guid.NewGuid();
        var eventoId2 = Guid.NewGuid();
        var asientoId1 = Guid.NewGuid();

        var entrada1 = Entrada.Crear(eventoId1, usuarioId, 100.00m, asientoId1, "TICKET-ABC123-4567");
        await Task.Delay(10); // Pequeña pausa para asegurar diferentes timestamps
        var entrada2 = Entrada.Crear(eventoId2, usuarioId, 75.50m, null, "TICKET-DEF456-7890");
        entrada2.ConfirmarPago();

        var entradas = new List<Entrada> { entrada1, entrada2 };
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);

        // Verificar que está ordenado por fecha de compra descendente (más reciente primero)
        resultado[0].FechaCompra.Should().BeOnOrAfter(resultado[1].FechaCompra);

        // Verificar datos de la primera entrada
        var dto1 = resultado.First(x => x.Id == entrada1.Id);
        dto1.EventoId.Should().Be(eventoId1);
        dto1.AsientoId.Should().Be(asientoId1);
        dto1.MontoFinal.Should().Be(100.00m);
        dto1.Estado.Should().Be(EstadoEntrada.PendientePago);
        dto1.CodigoQr.Should().Be("TICKET-ABC123-4567");

        // Verificar datos de la segunda entrada
        var dto2 = resultado.First(x => x.Id == entrada2.Id);
        dto2.EventoId.Should().Be(eventoId2);
        dto2.AsientoId.Should().BeNull();
        dto2.MontoFinal.Should().Be(75.50m);
        dto2.Estado.Should().Be(EstadoEntrada.Pagada);
        dto2.CodigoQr.Should().Be("TICKET-DEF456-7890");

        // Verificar que se llamó al repositorio
        _mockRepositorio.Verify(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoUsuarioNoTieneEntradas_DebeRetornarListaVacia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

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
    public async Task Handle_ConEntradasEnDiferentesEstados_DebeRetornarTodasConEstadoCorrecto()
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
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(4);

        resultado.Should().Contain(x => x.Estado == EstadoEntrada.PendientePago);
        resultado.Should().Contain(x => x.Estado == EstadoEntrada.Pagada);
        resultado.Should().Contain(x => x.Estado == EstadoEntrada.Cancelada);
        resultado.Should().Contain(x => x.Estado == EstadoEntrada.Usada);
    }

    [Fact]
    public async Task Handle_ConMuchasEntradas_DebeRetornarTodasOrdenadas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradas = new List<Entrada>();

        // Crear 15 entradas para el usuario
        for (int i = 0; i < 15; i++)
        {
            var eventoId = Guid.NewGuid();
            var monto = 50.00m + (i * 10);
            var codigoQr = $"TICKET-{i:D3}123-4567";
            var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
            entradas.Add(entrada);
            await Task.Delay(5); // Asegurar diferentes timestamps
        }
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(15);

        // Verificar que está ordenado descendentemente por fecha
        for (int i = 0; i < resultado.Count - 1; i++)
        {
            resultado[i].FechaCompra.Should().BeOnOrAfter(resultado[i + 1].FechaCompra);
        }

        // Verificar que los montos son correctos
        for (int i = 0; i < 15; i++)
        {
            var expectedMonto = 50.00m + (i * 10);
            resultado.Should().Contain(x => x.MontoFinal == expectedMonto);
        }
    }

    [Fact]
    public async Task Handle_CuandoRepositorioLanzaExcepcion_DebePropagar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

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
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

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
        Assert.Throws<ArgumentNullException>(() => new ObtenerHistorialUsuarioQueryHandler(null!, _mockLogger.Object));
        Assert.Throws<ArgumentNullException>(() => new ObtenerHistorialUsuarioQueryHandler(_mockRepositorio.Object, null!));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_ConGuidVacio_DebeConsultarRepositorio(string guidString)
    {
        // Arrange
        var usuarioId = Guid.Parse(guidString);
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

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
    public async Task Handle_DebeMapearTodosLosCamposCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        var entrada = Entrada.Crear(eventoId, usuarioId, 150.00m, asientoId, "TICKET-XYZ789-0123");
        entrada.ConfirmarPago();

        var entradas = new List<Entrada> { entrada };
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(1);
        var dto = resultado.First();

        dto.Id.Should().Be(entrada.Id);
        dto.EventoId.Should().Be(eventoId);
        dto.AsientoId.Should().Be(asientoId);
        dto.Estado.Should().Be(EstadoEntrada.Pagada);
        dto.MontoFinal.Should().Be(150.00m);
        dto.CodigoQr.Should().Be("TICKET-XYZ789-0123");
        dto.FechaCompra.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dto.FechaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(2);

        var dtoConAsiento = resultado.First(x => x.AsientoId.HasValue);
        dtoConAsiento.AsientoId.Should().Be(asientoId);
        dtoConAsiento.MontoFinal.Should().Be(150.00m);

        var dtoGeneral = resultado.First(x => !x.AsientoId.HasValue);
        dtoGeneral.AsientoId.Should().BeNull();
        dtoGeneral.MontoFinal.Should().Be(100.00m);
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
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(3);

        resultado.Should().Contain(x => x.EventoId == eventoId1);
        resultado.Should().Contain(x => x.EventoId == eventoId2);
        resultado.Should().Contain(x => x.EventoId == eventoId3);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(999.99)]
    [InlineData(10000.00)]
    public async Task Handle_ConDiferentesMontos_DebeRetornarMontosCorrectamente(decimal monto)
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var codigoQr = "TICKET-PQR678-9012";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        var entradas = new List<Entrada> { entrada };
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().MontoFinal.Should().Be(monto);
    }

    [Fact]
    public async Task Handle_CamposOpcionales_DebenSerNullCuandoNoEstanDisponibles()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        var entrada = Entrada.Crear(eventoId, usuarioId, 100.00m, null, "TICKET-ABC123-4567");
        var entradas = new List<Entrada> { entrada };
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(1);
        var dto = resultado.First();

        // Campos que actualmente no están disponibles en la entidad
        dto.TituloEvento.Should().BeNull();
        dto.FechaEvento.Should().BeNull();
        dto.Sector.Should().BeNull();
        dto.Fila.Should().BeNull();
        dto.Numero.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DebeRegistrarLogDeInformacion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entrada = Entrada.Crear(Guid.NewGuid(), usuarioId, 100.00m, null, "TICKET-ABC123-4567");
        var entradas = new List<Entrada> { entrada };
        
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);

        _mockRepositorio
            .Setup(x => x.ObtenerPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradas);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Obteniendo historial")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
