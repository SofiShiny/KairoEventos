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

public class AsientoReservadoConsumerTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<AsientoReservadoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<AsientoReservadoEventoDominio>> _contextMock;
    private readonly AsientoReservadoConsumer _consumer;

    public AsientoReservadoConsumerTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<AsientoReservadoConsumer>>();
        _contextMock = new Mock<ConsumeContext<AsientoReservadoEventoDominio>>();
        _consumer = new AsientoReservadoConsumer(_repositorioMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_DebeIncrementarReservadosYDecrementarDisponibles()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
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

        var reporteVentas = new ReporteVentasDiarias
        {
            Fecha = DateTime.UtcNow.Date,
            CantidadReservas = 10
        };

        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TotalReservas = 25
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(reporteVentas);
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricas);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.AsientosReservados == 51 &&
            h.AsientosDisponibles == 49 &&
            h.PorcentajeOcupacion == 51.0
        )), Times.Once);

        _repositorioMock.Verify(x => x.ActualizarVentasDiariasAsync(It.Is<ReporteVentasDiarias>(r =>
            r.CantidadReservas == 11
        )), Times.Once);

        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.Is<MetricasEvento>(m =>
            m.TotalReservas == 26
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_SinReporteVentasExistente_DebeCrearNuevoReporte()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
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
            AsientosDisponibles = 190
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((ReporteVentasDiarias?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarVentasDiariasAsync(It.Is<ReporteVentasDiarias>(r =>
            r.CantidadReservas == 1 &&
            r.Fecha.Date == DateTime.UtcNow.Date &&
            r.ReservasPorCategoria != null
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ConAsientosDisponiblesEnCero_NoDebeDecrementarMas()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 3,
            Numero = 10
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 100,
            AsientosDisponibles = 0,
            PorcentajeOcupacion = 100.0
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new ReporteVentasDiarias { CantidadReservas = 0 });

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.AsientosReservados == 101 &&
            h.AsientosDisponibles == 0
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_SinHistorialExistente_DebeLogearWarning()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 4,
            Numero = 15
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new ReporteVentasDiarias { CantidadReservas = 5 });

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()), Times.Never);
        
        // Debe actualizar el reporte de ventas aunque no haya historial
        _repositorioMock.Verify(x => x.ActualizarVentasDiariasAsync(It.IsAny<ReporteVentasDiarias>()), Times.Once);
        
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
        var evento = new AsientoReservadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 5,
            Numero = 20
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 50,
            AsientosDisponibles = 50
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new ReporteVentasDiarias { CantidadReservas = 0 });
        _repositorioMock.Setup(x => x.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()), Times.Never);
    }

    [Fact]
    public async Task Consume_CuandoOcurreError_DebeRegistrarEnAuditoriaYRelanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 6,
            Numero = 25
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _consumer.Consume(_contextMock.Object));

        _repositorioMock.Verify(x => x.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log =>
            log.TipoOperacion == "ErrorProcesamiento" &&
            log.Entidad == "Asiento" &&
            log.Exitoso == false &&
            log.MensajeError == "Error de base de datos"
        )), Times.Once);
    }

    [Theory]
    [InlineData(100, 50, 51.0)]
    [InlineData(200, 100, 50.5)]
    [InlineData(50, 25, 52.0)]
    public async Task Consume_DebeCalcularPorcentajeCorrectamente(int capacidadTotal, int asientosReservados, double porcentajeEsperado)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 7,
            Numero = 30
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
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new ReporteVentasDiarias { CantidadReservas = 0 });

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            Math.Abs(h.PorcentajeOcupacion - porcentajeEsperado) < 0.01
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_DebeActualizarTimestampCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new AsientoReservadoEventoDominio
        {
            MapaId = eventoId,
            Fila = 8,
            Numero = 35
        };

        var historialExistente = new HistorialAsistencia
        {
            EventoId = eventoId,
            CapacidadTotal = 100,
            AsientosReservados = 25,
            AsientosDisponibles = 75,
            UltimaActualizacion = DateTime.UtcNow.AddHours(-2)
        };

        var reporteVentas = new ReporteVentasDiarias
        {
            Fecha = DateTime.UtcNow.Date,
            CantidadReservas = 5,
            UltimaActualizacion = DateTime.UtcNow.AddHours(-1)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);
        _repositorioMock.Setup(x => x.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historialExistente);
        _repositorioMock.Setup(x => x.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(reporteVentas);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _repositorioMock.Verify(x => x.ActualizarAsistenciaAsync(It.Is<HistorialAsistencia>(h =>
            h.UltimaActualizacion > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);

        _repositorioMock.Verify(x => x.ActualizarVentasDiariasAsync(It.Is<ReporteVentasDiarias>(r =>
            r.UltimaActualizacion > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);
    }
}