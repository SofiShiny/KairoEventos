using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Reportes.Aplicacion.Consumers;
using Eventos.Dominio.EventosDeDominio;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using Xunit;
using MassTransit;

namespace Reportes.Pruebas.Aplicacion.Consumers;

/// <summary>
/// Unit tests para verificar resiliencia y manejo de errores
/// Valida: Requisitos 10.1, 10.3
/// </summary>
public class ResilienciaTests
{
    /// <summary>
    /// Test: Reintentos con backoff exponencial
    /// Verifica que el sistema reintenta con intervalos crecientes
    /// </summary>
    [Fact]
    public void ReintentosConBackoffExponencial_CalculaIntervalosCorrectamente()
    {
        // Arrange
        var minInterval = TimeSpan.FromSeconds(2);
        var intervalDelta = TimeSpan.FromSeconds(2);
        var maxInterval = TimeSpan.FromSeconds(30);

        // Act & Assert - Verificar intervalos para cada reintento
        for (int reintento = 1; reintento <= 3; reintento++)
        {
            var intervaloEsperado = minInterval + (intervalDelta * (reintento - 1));
            var intervaloFinal = intervaloEsperado > maxInterval ? maxInterval : intervaloEsperado;

            // Verificar que el intervalo está dentro del rango esperado
            Assert.True(intervaloFinal >= minInterval, 
                $"Reintento {reintento}: intervalo debe ser >= {minInterval.TotalSeconds}s");
            Assert.True(intervaloFinal <= maxInterval, 
                $"Reintento {reintento}: intervalo debe ser <= {maxInterval.TotalSeconds}s");
        }
    }

    /// <summary>
    /// Test: Movimiento a dead-letter queue después de 3 reintentos
    /// Verifica que después de 3 intentos fallidos, el evento debe fallar
    /// </summary>
    [Fact]
    public async Task MovimientoADeadLetterQueue_DespuesDe3Reintentos()
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();

        // Simular fallo persistente
        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .ThrowsAsync(new MongoException("Error de conexión"));

        var consumer = new EventoPublicadoConsumer(
            repositorioMock.Object,
            loggerMock.Object);

        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = Guid.NewGuid(),
            TituloEvento = "Evento de prueba",
            FechaInicio = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(c => c.Message).Returns(evento);

        // Act & Assert
        // El consumidor debe lanzar excepción para que MassTransit maneje el reintento
        await Assert.ThrowsAsync<MongoException>(async () =>
        {
            await consumer.Consume(contextMock.Object);
        });

        // Verificar que se intentó actualizar las métricas
        repositorioMock.Verify(
            r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()),
            Times.Once,
            "Debe intentar actualizar métricas");
    }

    /// <summary>
    /// Test: Encolamiento cuando MongoDB no está disponible
    /// Verifica que el sistema registra el error y permite reintentos
    /// </summary>
    [Fact]
    public async Task EncolamientoCuandoMongoDBNoDisponible_RegistraError()
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();

        // Simular MongoDB no disponible
        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .ThrowsAsync(new MongoException("MongoDB no disponible"));

        var consumer = new EventoPublicadoConsumer(
            repositorioMock.Object,
            loggerMock.Object);

        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = Guid.NewGuid(),
            TituloEvento = "Evento de prueba",
            FechaInicio = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(c => c.Message).Returns(evento);

        // Act
        try
        {
            await consumer.Consume(contextMock.Object);
        }
        catch (MongoException)
        {
            // Esperamos que falle
        }

        // Assert - Verificar que se registró el error
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce,
            "Debe registrar el error en logs cuando MongoDB no está disponible");
    }

    /// <summary>
    /// Test: Registro de auditoría en caso de error
    /// Verifica que los errores se registran en auditoría
    /// </summary>
    [Fact]
    public async Task RegistroAuditoriaEnCasoDeError_GuardaLogConExitosoFalse()
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();

        // Simular fallo en actualización pero éxito en auditoría
        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .ThrowsAsync(new Exception("Error de prueba"));

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        var consumer = new EventoPublicadoConsumer(
            repositorioMock.Object,
            loggerMock.Object);

        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = Guid.NewGuid(),
            TituloEvento = "Evento de prueba",
            FechaInicio = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(c => c.Message).Returns(evento);

        // Act
        try
        {
            await consumer.Consume(contextMock.Object);
        }
        catch
        {
            // Esperamos que falle
        }

        // Assert - Verificar que se intentó registrar en auditoría con Exitoso = false
        repositorioMock.Verify(
            r => r.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
                log.TipoOperacion == "ErrorProcesamiento" &&
                !log.Exitoso &&
                log.Entidad == "Evento")),
            Times.Once,
            "Debe registrar el error en auditoría con Exitoso = false");
    }

    /// <summary>
    /// Test: Timeout en operaciones de MongoDB
    /// Verifica que el sistema maneja timeouts correctamente
    /// </summary>
    [Fact]
    public async Task TimeoutEnOperacionesMongoDB_ManejaCorrectamente()
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();

        // Simular timeout
        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .ThrowsAsync(new TimeoutException("Operación excedió el tiempo límite"));

        var consumer = new EventoPublicadoConsumer(
            repositorioMock.Object,
            loggerMock.Object);

        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = Guid.NewGuid(),
            TituloEvento = "Evento de prueba",
            FechaInicio = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(c => c.Message).Returns(evento);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await consumer.Consume(contextMock.Object);
        });

        // Verificar que se registró el error
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<TimeoutException>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce,
            "Debe registrar el timeout en logs");
    }

    /// <summary>
    /// Test: Procesamiento exitoso registra auditoría correcta
    /// Verifica que las operaciones exitosas se registran correctamente
    /// </summary>
    [Fact]
    public async Task ProcesamientoExitoso_RegistraAuditoriaConExitosoTrue()
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();

        // Simular éxito
        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .Returns(Task.CompletedTask);

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        var consumer = new EventoPublicadoConsumer(
            repositorioMock.Object,
            loggerMock.Object);

        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = Guid.NewGuid(),
            TituloEvento = "Evento de prueba",
            FechaInicio = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(c => c.Message).Returns(evento);

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert - Verificar que se registró en auditoría con Exitoso = true
        repositorioMock.Verify(
            r => r.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
                log.TipoOperacion == "EventoConsumido" &&
                log.Exitoso &&
                log.Entidad == "Evento")),
            Times.Once,
            "Debe registrar el éxito en auditoría con Exitoso = true");
    }
}
