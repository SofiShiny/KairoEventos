using Eventos.Dominio.EventosDeDominio;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Consumers;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Consumers;

public class EventoCanceladoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<EventoCanceladoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<EventoCanceladoEventoDominio>> _contextMock;
    private readonly EventoCanceladoConsumer _consumer;

    public EventoCanceladoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<EventoCanceladoConsumer>>();
        _contextMock = new Mock<ConsumeContext<EventoCanceladoEventoDominio>>();
        _consumer = new EventoCanceladoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_MetricasExisten_ActualizaEstadoACancelado()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Concierto de Rock"
        };

        var metricasExistentes = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Concierto de Rock",
            Estado = "Publicado",
            FechaCreacion = DateTime.UtcNow.AddDays(-7)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricasExistentes);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(
            It.Is<MetricasEvento>(m => 
                m.EventoId == eventoId &&
                m.Estado == "Cancelado" &&
                m.TituloEvento == "Concierto de Rock")),
            Times.Once);
    }

    [Fact]
    public async Task Consume_MetricasNoExisten_CreaMetricasConEstadoCancelado()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Festival de Jazz"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        MetricasEvento? metricasCapturadas = null;
        _repositorioMock.Setup(x => x.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .Callback<MetricasEvento>(m => metricasCapturadas = m)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        metricasCapturadas.Should().NotBeNull();
        metricasCapturadas!.EventoId.Should().Be(eventoId);
        metricasCapturadas.TituloEvento.Should().Be("Festival de Jazz");
        metricasCapturadas.Estado.Should().Be("Cancelado");
        metricasCapturadas.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Consume_EventoValido_RegistraLogAuditoriaExitoso()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Obra de Teatro"
        };

        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Obra de Teatro",
            Estado = "Publicado"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricas);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log =>
                log.TipoOperacion == "EventoConsumido" &&
                log.Entidad == "Evento" &&
                log.EntidadId == eventoId.ToString() &&
                log.Detalles.Contains("Obra de Teatro") &&
                log.Exitoso == true &&
                log.MensajeError == null)),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_RegistraLogAuditoriaError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Concierto Sinfónico"
        };

        var excepcion = new InvalidOperationException("Error de base de datos");

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ThrowsAsync(excepcion);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log =>
                log.TipoOperacion == "ErrorProcesamiento" &&
                log.Entidad == "Evento" &&
                log.EntidadId == eventoId.ToString() &&
                log.Exitoso == false &&
                log.MensajeError == "Error de base de datos")),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_LanzaExcepcionParaReintento()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Exposición de Arte"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _consumer.Consume(_contextMock.Object));

        exception.Message.Should().Be("Error de conexión");
    }

    [Fact]
    public async Task Consume_EventoValido_LogsInformacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Partido de Fútbol"
        };

        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Partido de Fútbol",
            Estado = "Publicado"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricas);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando evento EventoCancelado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Estado del evento actualizado a Cancelado")),
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
    public async Task Consume_MetricasNoExisten_LogsWarning()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Seminario Técnico"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Métricas no encontradas")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_LogsError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoCanceladoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Conferencia Internacional"
        };

        var excepcion = new InvalidOperationException("Error de prueba");

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ThrowsAsync(excepcion);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error procesando evento EventoCancelado")),
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
            new EventoCanceladoEventoDominio { EventoId = Guid.NewGuid(), TituloEvento = "Evento 1" },
            new EventoCanceladoEventoDominio { EventoId = Guid.NewGuid(), TituloEvento = "Evento 2" },
            new EventoCanceladoEventoDominio { EventoId = Guid.NewGuid(), TituloEvento = "Evento 3" }
        };

        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(It.IsAny<Guid>()))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        foreach (var evento in eventos)
        {
            _contextMock.Setup(x => x.Message).Returns(evento);
            await _consumer.Consume(_contextMock.Object);
        }

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()), Times.Exactly(3));
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(
            It.Is<LogAuditoria>(log => log.Exitoso == true)), Times.Exactly(3));
    }
}
