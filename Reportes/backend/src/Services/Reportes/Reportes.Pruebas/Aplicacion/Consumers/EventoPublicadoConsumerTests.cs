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

public class EventoPublicadoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<EventoPublicadoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<EventoPublicadoEventoDominio>> _contextMock;
    private readonly EventoPublicadoConsumer _consumer;

    public EventoPublicadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();
        _contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        _consumer = new EventoPublicadoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ConMetricasExistentes_DebeActualizarMetricas()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Concierto de Rock";
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        var metricasExistentes = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Título Anterior",
            FechaInicio = DateTime.UtcNow.AddDays(25),
            Estado = "Borrador",
            FechaCreacion = DateTime.UtcNow.AddDays(-5)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricasExistentes);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.EventoId == eventoId &&
            m.TituloEvento == tituloEvento &&
            m.FechaInicio == fechaInicio &&
            m.Estado == "Publicado"
        )), Times.Once);

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "EventoConsumido" &&
            log.Entidad == "Evento" &&
            log.EntidadId == eventoId.ToString() &&
            log.Exitoso == true
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_SinMetricasExistentes_DebeCrearNuevasMetricas()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Festival de Jazz";
        var fechaInicio = DateTime.UtcNow.AddDays(45);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.EventoId == eventoId &&
            m.TituloEvento == tituloEvento &&
            m.FechaInicio == fechaInicio &&
            m.Estado == "Publicado" &&
            m.FechaCreacion > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeRegistrarEnAuditoriaConDetallesCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Obra de Teatro Clásica";
        var fechaInicio = DateTime.UtcNow.AddDays(15);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.Timestamp > DateTime.UtcNow.AddMinutes(-1) &&
            log.TipoOperacion == "EventoConsumido" &&
            log.Entidad == "Evento" &&
            log.EntidadId == eventoId.ToString() &&
            log.Detalles.Contains(tituloEvento) &&
            log.Exitoso == true
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoOcurreError_DebeRegistrarEnAuditoriaYRelanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Conferencia Técnica";
        var fechaInicio = DateTime.UtcNow.AddDays(20);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "ErrorProcesamiento" &&
            log.Entidad == "Evento" &&
            log.EntidadId == eventoId.ToString() &&
            log.Exitoso == false &&
            log.MensajeError == "Error de base de datos" &&
            log.Detalles.Contains(tituloEvento)
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeLogearInformacionCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Exposición de Arte";
        var fechaInicio = DateTime.UtcNow.AddDays(10);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert - Verificar que se logeó la información inicial
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando evento EventoPublicado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar que se logeó el éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("procesado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ConMetricasExistentes_DebePreservarFechaCreacionOriginal()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Seminario Empresarial";
        var fechaInicio = DateTime.UtcNow.AddDays(35);
        var fechaCreacionOriginal = DateTime.UtcNow.AddDays(-10);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        var metricasExistentes = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Título Anterior",
            FechaInicio = DateTime.UtcNow.AddDays(30),
            Estado = "Borrador",
            FechaCreacion = fechaCreacionOriginal
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricasExistentes);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.FechaCreacion == fechaCreacionOriginal
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeActualizarTodosLosCamposRelevantes()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Maratón de Programación";
        var fechaInicio = DateTime.UtcNow.AddDays(60);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        var metricasExistentes = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Título Diferente",
            FechaInicio = DateTime.UtcNow.AddDays(50),
            Estado = "Borrador",
            TotalReservas = 25,
            TotalAsistentes = 15,
            FechaCreacion = DateTime.UtcNow.AddDays(-3)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricasExistentes);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.EventoId == eventoId &&
            m.TituloEvento == tituloEvento &&
            m.FechaInicio == fechaInicio &&
            m.Estado == "Publicado" &&
            m.TotalReservas == 25 && // Debe preservar valores existentes
            m.TotalAsistentes == 15 // Debe preservar valores existentes
        )), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Consume_ConTituloVacioONulo_DebeProcesarCorrectamente(string? titulo)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var fechaInicio = DateTime.UtcNow.AddDays(40);
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = titulo!,
            FechaInicio = fechaInicio
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.TituloEvento == titulo &&
            m.Estado == "Publicado"
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ConFechaEnElPasado_DebeProcesarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var tituloEvento = "Evento Histórico";
        var fechaInicio = DateTime.UtcNow.AddDays(-5); // Fecha en el pasado
        
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = tituloEvento,
            FechaInicio = fechaInicio
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.FechaInicio == fechaInicio &&
            m.Estado == "Publicado"
        )), Times.Once);
    }
}