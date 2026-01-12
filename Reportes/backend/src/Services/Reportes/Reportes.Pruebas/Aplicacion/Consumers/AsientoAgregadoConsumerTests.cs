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

public class AsientoAgregadoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<AsientoAgregadoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<AsientoAgregadoEventoDominio>> _contextMock;
    private readonly AsientoAgregadoConsumer _consumer;

    public AsientoAgregadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<AsientoAgregadoConsumer>>();
        _contextMock = new Mock<ConsumeContext<AsientoAgregadoEventoDominio>>();
        _consumer = new AsientoAgregadoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_EventoValido_IncrementaCapacidadTotal()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 1,
            Numero = 5,
            Categoria = "VIP"
        };

        var historial = new HistorialAsistencia
        {
            EventoId = mapaId,
            CapacidadTotal = 100,
            AsientosDisponibles = 50
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ReturnsAsync(historial);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(
            It.Is<HistorialAsistencia>(h => h.CapacidadTotal == 101)),
            Times.Once);
    }

    [Fact]
    public async Task Consume_EventoValido_IncrementaAsientosDisponibles()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 2,
            Numero = 10,
            Categoria = "General"
        };

        var historial = new HistorialAsistencia
        {
            EventoId = mapaId,
            CapacidadTotal = 100,
            AsientosDisponibles = 50
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ReturnsAsync(historial);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(
            It.Is<HistorialAsistencia>(h => h.AsientosDisponibles == 51)),
            Times.Once);
    }

    [Fact]
    public async Task Consume_EventoValido_ActualizaTimestamp()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 3,
            Numero = 15,
            Categoria = "VIP"
        };

        var historial = new HistorialAsistencia
        {
            EventoId = mapaId,
            CapacidadTotal = 100,
            AsientosDisponibles = 50,
            UltimaActualizacion = DateTime.UtcNow.AddHours(-1)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ReturnsAsync(historial);

        HistorialAsistencia? historialCapturado = null;
        _repositorioMock.Setup(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()))
            .Callback<HistorialAsistencia>(h => historialCapturado = h)
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        historialCapturado.Should().NotBeNull();
        historialCapturado!.UltimaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Consume_HistorialNoExiste_NoActualizaNada()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 1,
            Numero = 1,
            Categoria = "General"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_NoLanzaExcepcion()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 1,
            Numero = 1,
            Categoria = "VIP"
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ThrowsAsync(new InvalidOperationException("Error de prueba"));

        // Act
        var act = async () => await _consumer.Consume(_contextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_EventoValido_LogsDebug()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 5,
            Numero = 20,
            Categoria = "General"
        };

        var historial = new HistorialAsistencia
        {
            EventoId = mapaId,
            CapacidadTotal = 100,
            AsientosDisponibles = 50
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ReturnsAsync(historial);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando evento AsientoAgregado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Capacidad actualizada")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_ErrorEnProcesamiento_LogsError()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var evento = new AsientoAgregadoEventoDominio
        {
            MapaId = mapaId,
            Fila = 1,
            Numero = 1,
            Categoria = "VIP"
        };

        var excepcion = new InvalidOperationException("Error de prueba");

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ThrowsAsync(excepcion);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error procesando evento AsientoAgregado")),
                excepcion,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_MultiplesAsientos_IncrementaCapacidadCorrectamente()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var historial = new HistorialAsistencia
        {
            EventoId = mapaId,
            CapacidadTotal = 100,
            AsientosDisponibles = 50
        };

        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(mapaId))
            .ReturnsAsync(historial);

        var eventos = new[]
        {
            new AsientoAgregadoEventoDominio { MapaId = mapaId, Fila = 1, Numero = 1, Categoria = "VIP" },
            new AsientoAgregadoEventoDominio { MapaId = mapaId, Fila = 1, Numero = 2, Categoria = "VIP" },
            new AsientoAgregadoEventoDominio { MapaId = mapaId, Fila = 1, Numero = 3, Categoria = "General" }
        };

        // Act
        foreach (var evento in eventos)
        {
            _contextMock.Setup(x => x.Message).Returns(evento);
            await _consumer.Consume(_contextMock.Object);
            historial.CapacidadTotal++; // Simular incremento
            historial.AsientosDisponibles++;
        }

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Exactly(3));
    }
}
