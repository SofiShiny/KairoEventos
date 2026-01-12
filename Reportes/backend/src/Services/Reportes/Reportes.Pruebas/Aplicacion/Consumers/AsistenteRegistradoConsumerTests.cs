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

public class AsistenteRegistradoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<AsistenteRegistradoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<AsistenteRegistradoEventoDominio>> _contextMock;
    private readonly AsistenteRegistradoConsumer _consumer;

    public AsistenteRegistradoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<AsistenteRegistradoConsumer>>();
        _contextMock = new Mock<ConsumeContext<AsistenteRegistradoEventoDominio>>();
        _consumer = new AsistenteRegistradoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_DebeAgregarNuevoAsistente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario123";
        var nombreUsuario = "Juan Pérez";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 5,
            Asistentes = new List<RegistroAsistente>
            {
                new() { UsuarioId = "usuario456", NombreUsuario = "María García" }
            }
        };

        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TotalAsistentes = 10
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricas);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.TotalAsistentesRegistrados == 6 &&
            h.Asistentes.Count == 2 &&
            h.Asistentes.Any(a => a.UsuarioId == usuarioId && a.NombreUsuario == nombreUsuario)
        )), Times.Once);

        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.TotalAsistentes == 11
        )), Times.Once);

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "EventoConsumido" &&
            log.Entidad == "Asistente" &&
            log.EntidadId == usuarioId &&
            log.Exitoso == true
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_SinHistorialExistente_DebeCrearNuevoHistorial()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario789";
        var nombreUsuario = "Carlos López";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.EventoId == eventoId &&
            h.TotalAsistentesRegistrados == 1 &&
            h.Asistentes.Count == 1 &&
            h.Asistentes.First().UsuarioId == usuarioId &&
            h.Asistentes.First().NombreUsuario == nombreUsuario
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ConAsistenteYaRegistrado_NoDebeAgregarDuplicado()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario123";
        var nombreUsuario = "Juan Pérez";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 5,
            Asistentes = new List<RegistroAsistente>
            {
                new() { UsuarioId = usuarioId, NombreUsuario = nombreUsuario },
                new() { UsuarioId = "usuario456", NombreUsuario = "María García" }
            }
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Never);
        
        // Debe registrar en auditoría aunque no actualice
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "EventoConsumido" &&
            log.Exitoso == true
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_SinMetricasExistentes_NoDebeActualizarMetricas()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario999";
        var nombreUsuario = "Ana Martínez";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 2,
            Asistentes = new List<RegistroAsistente>()
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()), Times.Never);
        
        // Debe actualizar el historial
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeActualizarTimestamp()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario555";
        var nombreUsuario = "Pedro Rodríguez";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 0,
            Asistentes = new List<RegistroAsistente>(),
            UltimaActualizacion = DateTime.UtcNow.AddHours(-2)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.UltimaActualizacion > DateTime.UtcNow.AddMinutes(-1) &&
            h.Asistentes.First().FechaRegistro > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoOcurreError_DebeRegistrarEnAuditoriaYRelanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario777";
        var nombreUsuario = "Laura Sánchez";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "ErrorProcesamiento" &&
            log.Entidad == "Asistente" &&
            log.EntidadId == usuarioId &&
            log.Exitoso == false &&
            log.MensajeError == "Error de conexión"
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeLogearInformacionCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario888";
        var nombreUsuario = "Roberto González";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 3,
            Asistentes = new List<RegistroAsistente>()
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert - Verificar que se logeó la información inicial
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando evento AsistenteRegistrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        // Verificar que se logeó el éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("registrado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Consume_ConAsistenteYaRegistrado_DebeLogearWarning()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario111";
        var nombreUsuario = "Sofía Herrera";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 1,
            Asistentes = new List<RegistroAsistente>
            {
                new() { UsuarioId = usuarioId, NombreUsuario = nombreUsuario }
            }
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert - Verificar que se logeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ya está registrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_DebeCrearRegistroAsistenteConDatosCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = "usuario222";
        var nombreUsuario = "Diego Morales";
        
        var evento = new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            TotalAsistentesRegistrados = 0,
            Asistentes = new List<RegistroAsistente>()
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.Asistentes.Count == 1 &&
            h.Asistentes.First().UsuarioId == usuarioId &&
            h.Asistentes.First().NombreUsuario == nombreUsuario &&
            h.Asistentes.First().FechaRegistro > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);
    }
}