using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Jobs;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Jobs;

/// <summary>
/// Unit tests para el job de consolidación de reportes.
/// Valida: Requisitos 4.4
/// </summary>
public class JobGenerarReportesConsolidadosTests
{
    private readonly Mock<IRepositorioReportesLectura> _repositorioMock;
    private readonly Mock<ILogger<JobGenerarReportesConsolidados>> _loggerMock;
    private readonly JobGenerarReportesConsolidados _job;

    public JobGenerarReportesConsolidadosTests()
    {
        _repositorioMock = new Mock<IRepositorioReportesLectura>();
        _loggerMock = new Mock<ILogger<JobGenerarReportesConsolidados>>();
        _job = new JobGenerarReportesConsolidados(_repositorioMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Test de ejecución exitosa: Verifica que el job se ejecuta correctamente
    /// con datos válidos y guarda el reporte consolidado.
    /// </summary>
    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_GuardaReporteConsolidado()
    {
        // Arrange
        var ayer = DateTime.UtcNow.Date.AddDays(-1);
        
        var ventasDiarias = new List<ReporteVentasDiarias>
        {
            new ReporteVentasDiarias
            {
                Fecha = ayer,
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 1",
                CantidadReservas = 50,
                TotalIngresos = 5000m,
                ReservasPorCategoria = new Dictionary<string, int>
                {
                    { "VIP", 10 },
                    { "General", 40 }
                }
            },
            new ReporteVentasDiarias
            {
                Fecha = ayer,
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 2",
                CantidadReservas = 30,
                TotalIngresos = 3000m,
                ReservasPorCategoria = new Dictionary<string, int>
                {
                    { "VIP", 5 },
                    { "General", 25 }
                }
            }
        };

        var metricas = new List<MetricasEvento>
        {
            new MetricasEvento
            {
                EventoId = ventasDiarias[0].EventoId,
                TituloEvento = "Evento 1",
                FechaInicio = ayer,
                TotalAsistentes = 50
            },
            new MetricasEvento
            {
                EventoId = ventasDiarias[1].EventoId,
                TituloEvento = "Evento 2",
                FechaInicio = ayer,
                TotalAsistentes = 30
            }
        };

        ReporteConsolidado? reporteGuardado = null;

        _repositorioMock
            .Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventasDiarias);

        _repositorioMock
            .Setup(r => r.ObtenerTodasMetricasAsync())
            .ReturnsAsync(metricas);

        _repositorioMock
            .Setup(r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()))
            .Callback<ReporteConsolidado>(r => reporteGuardado = r)
            .Returns(Task.CompletedTask);

        _repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        // Act
        await _job.EjecutarAsync();

        // Assert
        Assert.NotNull(reporteGuardado);
        Assert.Equal(8000m, reporteGuardado.TotalIngresos);
        Assert.Equal(80, reporteGuardado.TotalReservas);
        Assert.Equal(2, reporteGuardado.TotalEventos);
        Assert.Equal(40, reporteGuardado.PromedioAsistenciaEvento);
        
        // Verificar que se guardó el reporte
        _repositorioMock.Verify(
            r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()),
            Times.Once);

        // Verificar que se registró en auditoría
        _repositorioMock.Verify(
            r => r.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log => 
                log.TipoOperacion == "ReporteGenerado" && log.Exitoso)),
            Times.Once);
    }

    /// <summary>
    /// Test de manejo de errores: Verifica que el job maneja correctamente
    /// errores de MongoDB y registra el fallo en auditoría.
    /// </summary>
    [Fact]
    public async Task EjecutarAsync_ErrorMongoDB_RegistraEnAuditoria()
    {
        // Arrange
        _repositorioMock
            .Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Error de conexión a MongoDB"));

        _repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _job.EjecutarAsync());

        // Verificar que se registró el error en auditoría
        _repositorioMock.Verify(
            r => r.RegistrarLogAuditoriaAsync(It.Is<LogAuditoria>(log => 
                log.TipoOperacion == "ErrorProcesamiento" && 
                !log.Exitoso &&
                log.MensajeError == "Error de conexión a MongoDB")),
            Times.Once);
    }

    /// <summary>
    /// Test de registro en auditoría: Verifica que el job registra
    /// correctamente la operación exitosa en auditoría.
    /// </summary>
    [Fact]
    public async Task EjecutarAsync_Exitoso_RegistraLogAuditoriaConDetalles()
    {
        // Arrange
        var ventasDiarias = new List<ReporteVentasDiarias>
        {
            new ReporteVentasDiarias
            {
                Fecha = DateTime.UtcNow.Date.AddDays(-1),
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento Test",
                CantidadReservas = 100,
                TotalIngresos = 10000m,
                ReservasPorCategoria = new Dictionary<string, int> { { "General", 100 } }
            }
        };

        var metricas = new List<MetricasEvento>
        {
            new MetricasEvento
            {
                EventoId = ventasDiarias[0].EventoId,
                TituloEvento = "Evento Test",
                FechaInicio = DateTime.UtcNow.Date.AddDays(-1),
                TotalAsistentes = 100
            }
        };

        LogAuditoria? logGuardado = null;

        _repositorioMock
            .Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventasDiarias);

        _repositorioMock
            .Setup(r => r.ObtenerTodasMetricasAsync())
            .ReturnsAsync(metricas);

        _repositorioMock
            .Setup(r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()))
            .Returns(Task.CompletedTask);

        _repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Callback<LogAuditoria>(log => logGuardado = log)
            .Returns(Task.CompletedTask);

        // Act
        await _job.EjecutarAsync();

        // Assert
        Assert.NotNull(logGuardado);
        Assert.Equal("ReporteGenerado", logGuardado.TipoOperacion);
        Assert.Equal("ReporteConsolidado", logGuardado.Entidad);
        Assert.True(logGuardado.Exitoso);
        Assert.Null(logGuardado.MensajeError);
        Assert.Contains("Consolidación exitosa", logGuardado.Detalles);
        Assert.Contains("Ingresos: 10000", logGuardado.Detalles);
        Assert.Contains("Reservas: 100", logGuardado.Detalles);
        Assert.Contains("Eventos: 1", logGuardado.Detalles);
    }

    /// <summary>
    /// Test con datos vacíos: Verifica que el job maneja correctamente
    /// el caso cuando no hay datos para consolidar.
    /// </summary>
    [Fact]
    public async Task EjecutarAsync_SinDatos_GeneraReporteVacio()
    {
        // Arrange
        var ventasDiarias = new List<ReporteVentasDiarias>();
        var metricas = new List<MetricasEvento>();

        ReporteConsolidado? reporteGuardado = null;

        _repositorioMock
            .Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventasDiarias);

        _repositorioMock
            .Setup(r => r.ObtenerTodasMetricasAsync())
            .ReturnsAsync(metricas);

        _repositorioMock
            .Setup(r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()))
            .Callback<ReporteConsolidado>(r => reporteGuardado = r)
            .Returns(Task.CompletedTask);

        _repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        // Act
        await _job.EjecutarAsync();

        // Assert
        Assert.NotNull(reporteGuardado);
        Assert.Equal(0m, reporteGuardado.TotalIngresos);
        Assert.Equal(0, reporteGuardado.TotalReservas);
        Assert.Equal(0, reporteGuardado.TotalEventos);
        Assert.Equal(0, reporteGuardado.PromedioAsistenciaEvento);
        Assert.Empty(reporteGuardado.IngresosPorCategoria);
    }

    /// <summary>
    /// Test de cálculo de ingresos por categoría: Verifica que el job
    /// calcula correctamente los ingresos distribuidos por categoría.
    /// </summary>
    [Fact]
    public async Task EjecutarAsync_ConMultiplesCategorias_CalculaIngresosPorCategoria()
    {
        // Arrange
        var ayer = DateTime.UtcNow.Date.AddDays(-1);
        
        var ventasDiarias = new List<ReporteVentasDiarias>
        {
            new ReporteVentasDiarias
            {
                Fecha = ayer,
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 1",
                CantidadReservas = 100,
                TotalIngresos = 10000m,
                ReservasPorCategoria = new Dictionary<string, int>
                {
                    { "VIP", 20 },      // 20% de 10000 = 2000
                    { "General", 80 }   // 80% de 10000 = 8000
                }
            }
        };

        var metricas = new List<MetricasEvento>
        {
            new MetricasEvento
            {
                EventoId = ventasDiarias[0].EventoId,
                FechaInicio = ayer,
                TotalAsistentes = 100
            }
        };

        ReporteConsolidado? reporteGuardado = null;

        _repositorioMock
            .Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventasDiarias);

        _repositorioMock
            .Setup(r => r.ObtenerTodasMetricasAsync())
            .ReturnsAsync(metricas);

        _repositorioMock
            .Setup(r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()))
            .Callback<ReporteConsolidado>(r => reporteGuardado = r)
            .Returns(Task.CompletedTask);

        _repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        // Act
        await _job.EjecutarAsync();

        // Assert
        Assert.NotNull(reporteGuardado);
        Assert.Equal(2, reporteGuardado.IngresosPorCategoria.Count);
        Assert.True(reporteGuardado.IngresosPorCategoria.ContainsKey("VIP"));
        Assert.True(reporteGuardado.IngresosPorCategoria.ContainsKey("General"));
        
        // Verificar proporciones (con tolerancia para decimales)
        Assert.True(Math.Abs(reporteGuardado.IngresosPorCategoria["VIP"] - 2000m) < 0.01m);
        Assert.True(Math.Abs(reporteGuardado.IngresosPorCategoria["General"] - 8000m) < 0.01m);
    }

    /// <summary>
    /// Test de error en auditoría: Verifica que si falla el registro de auditoría,
    /// el error se registra en logs pero no impide la ejecución del job.
    /// </summary>
    [Fact]
    public async Task EjecutarAsync_ErrorEnAuditoria_NoImpidenEjecucion()
    {
        // Arrange
        var ventasDiarias = new List<ReporteVentasDiarias>();
        var metricas = new List<MetricasEvento>();

        _repositorioMock
            .Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventasDiarias);

        _repositorioMock
            .Setup(r => r.ObtenerTodasMetricasAsync())
            .ReturnsAsync(metricas);

        _repositorioMock
            .Setup(r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()))
            .Returns(Task.CompletedTask);

        // Simular error en auditoría
        _repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .ThrowsAsync(new Exception("Error en auditoría"));

        // Act & Assert - El job debe completarse a pesar del error en auditoría
        // (el error se registra en logs pero no se propaga)
        await _job.EjecutarAsync();

        // Verificar que se intentó guardar el reporte
        _repositorioMock.Verify(
            r => r.GuardarReporteConsolidadoAsync(It.IsAny<ReporteConsolidado>()),
            Times.Once);
    }
}
