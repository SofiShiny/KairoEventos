using Asientos.Dominio.EventosDominio;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Consumers;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Consumers;

public class MapaAsientosCreadoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<MapaAsientosCreadoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<MapaAsientosCreadoEventoDominio>> _contextMock;
    private readonly MapaAsientosCreadoConsumer _consumer;

    public MapaAsientosCreadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<MapaAsientosCreadoConsumer>>();
        _contextMock = new Mock<ConsumeContext<MapaAsientosCreadoEventoDominio>>();
        _consumer = new MapaAsientosCreadoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_EventoValido_ProcesaExitosamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(
            It.Is<HistorialAsistencia>(h => 
                h.EventoId == eventoId &&
                h.CapacidadTotal == 0 &&
                h.AsientosReservados == 0 &&
                h.AsientosDisponibles == 0 &&
                h.PorcentajeOcupacion == 0)),
            Times.Once);
    }

    [Fact]
    public async Task Consume_HistorialNoExiste_CreaHistorialNuevo()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        HistorialAsistencia? historialCapturado = null;
        _repositorioMock.Setup(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()))
            .Callback<HistorialAsistencia>(h => historialCapturado = h)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        historialCapturado.Should().NotBeNull();
        historialCapturado!.EventoId.Should().Be(eventoId);
        historialCapturado.CapacidadTotal.Should().Be(0);
        historialCapturado.AsientosReservados.Should().Be(0);
        historialCapturado.AsientosDisponibles.Should().Be(0);
        historialCapturado.PorcentajeOcupacion.Should().Be(0);
        historialCapturado.Asistentes.Should().NotBeNull();
        historialCapturado.Asistentes.Should().BeEmpty();
        historialCapturado.UltimaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Consume_HistorialExiste_ActualizaHistorialExistente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        var historialExistente = new HistorialAsistencia
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 50,
            AsientosDisponibles = 50,
            PorcentajeOcupacion = 50,
            Asistentes = new List<RegistroAsistente>()
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        HistorialAsistencia? historialCapturado = null;
        _repositorioMock.Setup(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()))
            .Callback<HistorialAsistencia>(h => historialCapturado = h)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        historialCapturado.Should().NotBeNull();
        historialCapturado!.EventoId.Should().Be(eventoId);
        historialCapturado.CapacidadTotal.Should().Be(100); // Mantiene valores existentes
        historialCapturado.AsientosReservados.Should().Be(50);
        historialCapturado.UltimaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Consume_EventoValido_RegistraLogAuditoriaExitoso()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log =>
                log.TipoOperacion == "EventoConsumido" &&
                log.Entidad == "MapaAsientos" &&
                log.EntidadId == mapaId.ToString() &&
                log.Detalles.Contains(eventoId.ToString()) &&
                log.Exitoso == true &&
                log.MensajeError == null)),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_RegistraLogAuditoriaError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        var excepcion = new InvalidOperationException("Error de prueba");

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ThrowsAsync(excepcion);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log =>
                log.TipoOperacion == "ErrorProcesamiento" &&
                log.Entidad == "MapaAsientos" &&
                log.EntidadId == mapaId.ToString() &&
                log.Exitoso == false &&
                log.MensajeError == "Error de prueba")),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_LanzaExcepcionParaReintento()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ThrowsAsync(new InvalidOperationException("Error de prueba"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_contextMock.Object));

        exception.Message.Should().Be("Error de prueba");
    }

    [Fact]
    public async Task Consume_EventoValido_LogsInformacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando evento MapaAsientosCreado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("procesado exitosamente")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_LogsError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var mapaId = Guid.NewGuid();
        var evento = new MapaAsientosCreadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId
        };

        var excepcion = new InvalidOperationException("Error de prueba");

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ThrowsAsync(excepcion);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error procesando evento MapaAsientosCreado")),
                excepcion,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_MultiplesEventos_ProcesaCadaUnoIndependientemente()
    {
        // Arrange
        var eventos = new[]
        {
            new MapaAsientosCreadoEventoDominio { MapaId = Guid.NewGuid(), EventoId = Guid.NewGuid() },
            new MapaAsientosCreadoEventoDominio { MapaId = Guid.NewGuid(), EventoId = Guid.NewGuid() },
            new MapaAsientosCreadoEventoDominio { MapaId = Guid.NewGuid(), EventoId = Guid.NewGuid() }
        };

        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(It.IsAny<Guid>()))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        foreach (var evento in eventos)
        {
            _contextMock.Setup(x => x.Message).Returns(evento);
            await _consumer.Consume(_contextMock.Object);
        }

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Exactly(3));
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log => log.Exitoso == true)), Times.Exactly(3));
    }
}
