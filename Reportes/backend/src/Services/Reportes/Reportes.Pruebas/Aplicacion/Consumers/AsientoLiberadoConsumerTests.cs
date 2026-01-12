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

public class AsientoLiberadoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<AsientoLiberadoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<AsientoLiberadoEventoDominio>> _contextMock;
    private readonly AsientoLiberadoConsumer _consumer;

    public AsientoLiberadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<AsientoLiberadoConsumer>>();
        _contextMock = new Mock<ConsumeContext<AsientoLiberadoEventoDominio>>();
        _consumer = new AsientoLiberadoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_DebeDecrementarReservadosEIncrementarDisponibles()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 1,
            Numero = 1
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 50,
            AsientosDisponibles = 50,
            PorcentajeOcupacion = 50.0
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.AsientosReservados == 49 &&
            h.AsientosDisponibles == 51 &&
            h.PorcentajeOcupacion == 49.0
        )), Times.Once);

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "EventoConsumido" &&
            log.Entidad == "Asiento" &&
            log.Exitoso == true
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_DebeActualizarTimestamp()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 2,
            Numero = 5
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 200,
            AsientosReservados = 10,
            AsientosDisponibles = 190,
            UltimaActualizacion = DateTime.UtcNow.AddHours(-1)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.UltimaActualizacion > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ConAsientosReservadosEnCero_NoDebeDecrementarMas()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 3,
            Numero = 10
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 0,
            AsientosDisponibles = 100,
            PorcentajeOcupacion = 0.0
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.AsientosReservados == 0 &&
            h.AsientosDisponibles == 101
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_SinHistorialExistente_DebeLogearWarning()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 4,
            Numero = 15
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Never);
        
        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "EventoConsumido" &&
            log.Exitoso == true
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_CuandoOcurreError_DebeRegistrarEnAuditoriaYRelanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 5,
            Numero = 20
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "ErrorProcesamiento" &&
            log.Entidad == "Asiento" &&
            log.Exitoso == false &&
            log.MensajeError == "Error de conexión"
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ConCapacidadTotalCero_NoDebeCalcularPorcentaje()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 6,
            Numero = 25
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 0,
            AsientosReservados = 5,
            AsientosDisponibles = 0,
            PorcentajeOcupacion = 0.0
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.PorcentajeOcupacion == 0.0
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeLogearInformacionCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 7,
            Numero = 30
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 75,
            AsientosDisponibles = 25
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesando evento AsientoLiberado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        // Verificar que se logeó el éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("procesado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(100, 50, 49.0)]
    [InlineData(200, 100, 49.5)]
    [InlineData(50, 25, 48.0)]
    public async Task Consume_DebeCalcularPorcentajeCorrectamente(int capacidadTotal, int asientosReservados, double porcentajeEsperado)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoLiberadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 8,
            Numero = 35
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = capacidadTotal,
            AsientosReservados = asientosReservados,
            AsientosDisponibles = capacidadTotal - asientosReservados
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            Math.Abs(h.PorcentajeOcupacion - porcentajeEsperado) < 0.01
        )), Times.Once);
    }
}