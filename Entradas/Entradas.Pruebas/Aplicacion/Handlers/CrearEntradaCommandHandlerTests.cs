using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.Handlers;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;
using Entradas.Dominio.Eventos;
using Entradas.Dominio.Excepciones;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.Aplicacion.Handlers;

/// <summary>
/// Pruebas comprehensivas para CrearEntradaCommandHandler
/// Cubre todos los casos edge y escenarios de error
/// </summary>
public class CrearEntradaCommandHandlerTests
{
    private readonly Mock<IVerificadorEventos> _mockVerificadorEventos;
    private readonly Mock<IVerificadorAsientos> _mockVerificadorAsientos;
    private readonly Mock<IGeneradorCodigoQr> _mockGeneradorQr;
    private readonly Mock<IRepositorioEntradas> _mockRepositorio;
    private readonly Mock<IPublishEndpoint> _mockPublisher;
    private readonly Mock<ILogger<CrearEntradaCommandHandler>> _mockLogger;
    private readonly Mock<IEntradasMetrics> _mockMetrics;
    private readonly ActivitySource _activitySource;
    private readonly Mock<IServicioDescuentos> _mockServicioDescuentos;
    private readonly Mock<IAsientosService> _mockAsientosService;
    private readonly CrearEntradaCommandHandler _handler;

    public CrearEntradaCommandHandlerTests()
    {
        _mockVerificadorEventos = new Mock<IVerificadorEventos>();
        _mockVerificadorAsientos = new Mock<IVerificadorAsientos>();
        _mockGeneradorQr = new Mock<IGeneradorCodigoQr>();
        _mockRepositorio = new Mock<IRepositorioEntradas>();
        _mockPublisher = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<CrearEntradaCommandHandler>>();
        _mockMetrics = new Mock<IEntradasMetrics>();
        _activitySource = new ActivitySource("Test");
        _mockServicioDescuentos = new Mock<IServicioDescuentos>();
        _mockAsientosService = new Mock<IAsientosService>();

        _handler = new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object,
            _mockVerificadorAsientos.Object,
            _mockGeneradorQr.Object,
            _mockRepositorio.Object,
            _mockPublisher.Object,
            _mockLogger.Object,
            _mockMetrics.Object,
            _activitySource,
            _mockServicioDescuentos.Object,
            _mockAsientosService.Object);

        // Setup default behaviors for basic success paths
        _mockVerificadorEventos.Setup(v => v.ObtenerInfoEventoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new EventoInfo(Guid.NewGuid(), "Evento Test", DateTime.UtcNow.AddDays(1), true, 100m));
        
        _mockAsientosService.Setup(s => s.GetAsientoByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new AsientoDto(Guid.NewGuid(), "A1", 100m, "General", 1, 1, true));
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DebeCrearEntradaExitosamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 100.50m;
        var codigoQr = "TICKET-ABC123-4567";

        var command = new CrearEntradaCommand(eventoId, usuarioId, asientoId);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockVerificadorAsientos
            .Setup(x => x.AsientoDisponibleAsync(eventoId, asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockAsientosService
            .Setup(x => x.GetAsientoByIdAsync(asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AsientoDto(asientoId, "Asiento 1", monto, "General", 1, 1, true));

        _mockGeneradorQr
            .Setup(x => x.GenerarCodigoUnico())
            .Returns(codigoQr);

        var entradaGuardada = Entrada.Crear(eventoId, usuarioId, monto, asientoId, codigoQr);
        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradaGuardada);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EventoId.Should().Be(eventoId);
        resultado.UsuarioId.Should().Be(usuarioId);
        resultado.AsientoId.Should().Be(asientoId);
        resultado.Monto.Should().Be(monto);
        resultado.CodigoQr.Should().Be(codigoQr);
        resultado.Estado.Should().Be(EstadoEntrada.PendientePago);

        // Verificar que se llamaron todos los servicios
        _mockVerificadorEventos.Verify(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()), Times.Once);
        _mockVerificadorAsientos.Verify(x => x.AsientoDisponibleAsync(eventoId, asientoId, It.IsAny<CancellationToken>()), Times.Once);
        _mockGeneradorQr.Verify(x => x.GenerarCodigoUnico(), Times.Once);
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(x => x.Publish(It.IsAny<EntradaCreadaEvento>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verificar métricas
        _mockMetrics.Verify(x => x.RecordServicioExternoDuration("eventos", It.IsAny<double>(), "success"), Times.Once);
        _mockMetrics.Verify(x => x.RecordServicioExternoDuration("asientos", It.IsAny<double>(), "success"), Times.Once);
        _mockMetrics.Verify(x => x.IncrementEntradasCreadas(eventoId.ToString(), EstadoEntrada.PendientePago.ToString()), Times.Once);
        _mockMetrics.Verify(x => x.RecordCreacionDuration(It.IsAny<double>(), "success"), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEntradaGeneral_NoDebeVerificarAsiento()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 75.00m;
        var codigoQr = "TICKET-DEF456-7890";

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockVerificadorEventos
            .Setup(x => x.ObtenerInfoEventoAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventoInfo(eventoId, "Evento Test", DateTime.Now, true, monto));

        _mockGeneradorQr
            .Setup(x => x.GenerarCodigoUnico())
            .Returns(codigoQr);

        var entradaGuardada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradaGuardada);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.AsientoId.Should().BeNull();

        // Verificar que NO se llamó al verificador de asientos
        _mockVerificadorAsientos.Verify(x => x.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // Pero sí se verificó el evento
        _mockVerificadorEventos.Verify(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoEventoNoDisponible_DebeLanzarEventoNoDisponibleException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EventoNoDisponibleException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.EventoId.Should().Be(eventoId);
        exception.Message.Should().Contain(eventoId.ToString());

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.RecordServicioExternoDuration("eventos", It.IsAny<double>(), "not_found"), Times.Once);
        _mockMetrics.Verify(x => x.IncrementValidacionExternaError("eventos", "evento_no_disponible"), Times.Once);
        _mockMetrics.Verify(x => x.RecordCreacionDuration(It.IsAny<double>(), "evento_no_disponible"), Times.Once);

        // Verificar que no se llamaron servicios posteriores
        _mockVerificadorAsientos.Verify(x => x.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockGeneradorQr.Verify(x => x.GenerarCodigoUnico(), Times.Never);
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoAsientoNoDisponible_DebeLanzarAsientoNoDisponibleException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 150.00m;

        var command = new CrearEntradaCommand(eventoId, usuarioId, asientoId);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockVerificadorAsientos
            .Setup(x => x.AsientoDisponibleAsync(eventoId, asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AsientoNoDisponibleException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.EventoId.Should().Be(eventoId);
        exception.AsientoId.Should().Be(asientoId);

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.RecordServicioExternoDuration("eventos", It.IsAny<double>(), "success"), Times.Once);
        _mockMetrics.Verify(x => x.RecordServicioExternoDuration("asientos", It.IsAny<double>(), "not_available"), Times.Once);
        _mockMetrics.Verify(x => x.IncrementValidacionExternaError("asientos", "asiento_no_disponible"), Times.Once);
        _mockMetrics.Verify(x => x.RecordCreacionDuration(It.IsAny<double>(), "asiento_no_disponible"), Times.Once);

        // Verificar que no se llamaron servicios posteriores
        _mockGeneradorQr.Verify(x => x.GenerarCodigoUnico(), Times.Never);
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CuandoVerificadorEventosLanzaExcepcion_DebeRegistrarMetricasYPropagar()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        var excepcionOriginal = new HttpRequestException("Servicio no disponible");
        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionOriginal);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(excepcionOriginal);

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.RecordCreacionDuration(It.IsAny<double>(), "error"), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoRepositorioFalla_DebeRegistrarMetricasYPropagar()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;
        var codigoQr = "TICKET-GHI789-0123";

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockGeneradorQr
            .Setup(x => x.GenerarCodigoUnico())
            .Returns(codigoQr);

        var excepcionRepositorio = new InvalidOperationException("Error de base de datos");
        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionRepositorio);

        // Act & Assert
        // El handler ahora calcula el monto desde el servicio de asientos
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(excepcionRepositorio);

        // Verificar métricas de error
        _mockMetrics.Verify(x => x.RecordCreacionDuration(It.IsAny<double>(), "error"), Times.Once);
    }

    [Fact]
    public async Task Handle_CuandoPublisherFalla_DebeRegistrarMetricasYPropagar()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;
        var codigoQr = "TICKET-JKL012-3456";

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockGeneradorQr
            .Setup(x => x.GenerarCodigoUnico())
            .Returns(codigoQr);

        var entradaGuardada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradaGuardada);

        var excepcionPublisher = new InvalidOperationException("Error de RabbitMQ");
        _mockPublisher
            .Setup(x => x.Publish(It.IsAny<EntradaCreadaEvento>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionPublisher);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(excepcionPublisher);

        // Verificar que se guardó la entrada antes del error
        _mockRepositorio.Verify(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar métricas de error
        _mockMetrics.Verify(x => x.RecordCreacionDuration(It.IsAny<double>(), "error"), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DebeRespetarCancelacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cts.Token));
    }

    [Fact]
    public void Constructor_ConParametrosNulos_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            null!, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, null!, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, null!, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            null!, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, null!, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, null!, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            null!, _activitySource, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, null!, _mockServicioDescuentos.Object, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, null!, _mockAsientosService.Object));

        Assert.Throws<ArgumentNullException>(() => new CrearEntradaCommandHandler(
            _mockVerificadorEventos.Object, _mockVerificadorAsientos.Object, _mockGeneradorQr.Object, 
            _mockRepositorio.Object, _mockPublisher.Object, _mockLogger.Object, 
            _mockMetrics.Object, _activitySource, _mockServicioDescuentos.Object, null!));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100.5)]
    [InlineData(0)]
    public async Task Handle_ConMontoInvalido_DebeLanzarExcepcion(decimal monto)
    {
        // Arrange - La entidad de dominio valida el monto y debe lanzar excepción
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var codigoQr = "TICKET-MNO345-6789";

        var command = new CrearEntradaCommand(eventoId, usuarioId, null);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Forzar que el servicio retorne el monto inválido para gatillar la validación de dominio
        _mockVerificadorEventos
            .Setup(x => x.ObtenerInfoEventoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventoInfo(eventoId, "Evento Test", DateTime.UtcNow.AddDays(1), true, monto));

        _mockGeneradorQr
            .Setup(x => x.GenerarCodigoUnico())
            .Returns(codigoQr);

        // Act & Assert - Debe lanzar excepción por monto inválido
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El monto debe ser mayor a cero. (Parameter 'monto')");
    }

    [Fact]
    public async Task Handle_DebePublicarEventoConDatosCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 200.75m;
        var codigoQr = "TICKET-PQR678-9012";

        var command = new CrearEntradaCommand(eventoId, usuarioId, asientoId);

        _mockVerificadorEventos
            .Setup(x => x.EventoExisteYDisponibleAsync(eventoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockVerificadorAsientos
            .Setup(x => x.AsientoDisponibleAsync(eventoId, asientoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockGeneradorQr
            .Setup(x => x.GenerarCodigoUnico())
            .Returns(codigoQr);

        var entradaGuardada = Entrada.Crear(eventoId, usuarioId, monto, asientoId, codigoQr);
        _mockRepositorio
            .Setup(x => x.GuardarAsync(It.IsAny<Entrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entradaGuardada);

        EntradaCreadaEvento? eventoPublicado = null;
        _mockPublisher
            .Setup(x => x.Publish(It.IsAny<EntradaCreadaEvento>(), It.IsAny<CancellationToken>()))
            .Callback<EntradaCreadaEvento, CancellationToken>((evt, ct) => eventoPublicado = evt);
        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPublisher.Verify(x => x.Publish(It.Is<EntradaCreadaEvento>(e => 
            e.EntradaId == entradaGuardada.Id &&
            e.EventoId == eventoId &&
            e.UsuarioId == usuarioId &&
            e.Monto == monto &&
            e.CodigoQr == codigoQr &&
            e.FechaCreacion == entradaGuardada.FechaCompra), It.IsAny<CancellationToken>()), Times.Once);
    }
}