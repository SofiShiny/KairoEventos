using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.API.Controladores;
using Reportes.API.DTOs;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.API;

/// <summary>
/// Unit Tests para ReportesController
/// Valida casos específicos, edge cases y manejo de errores
/// </summary>
public class ReportesControllerTests
{
    private readonly Mock<IRepositorioReportesLectura> _mockRepositorio;
    private readonly Mock<ILogger<ReportesController>> _mockLogger;
    private readonly ReportesController _controller;

    public ReportesControllerTests()
    {
        _mockRepositorio = new Mock<IRepositorioReportesLectura>();
        _mockLogger = new Mock<ILogger<ReportesController>>();
        _controller = new ReportesController(_mockRepositorio.Object, _mockLogger.Object);
    }

    #region ObtenerResumenVentas Tests

    [Fact]
    public async Task ObtenerResumenVentas_FechaInicioMayorQueFin_Retorna400()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow;
        var fechaFin = fechaInicio.AddDays(-1);

        // Act
        var resultado = await _controller.ObtenerResumenVentas(fechaInicio, fechaFin);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task ObtenerResumenVentas_DatosValidos_RetornaResumen()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-7);
        var fechaFin = DateTime.UtcNow;
        var ventas = new List<ReporteVentasDiarias>
        {
            new ReporteVentasDiarias
            {
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 1",
                Fecha = fechaInicio,
                CantidadReservas = 10,
                TotalIngresos = 1000m,
                ReservasPorCategoria = new Dictionary<string, int> { { "VIP", 5 }, { "General", 5 } }
            }
        };

        _mockRepositorio.Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventas);

        // Act
        var resultado = await _controller.ObtenerResumenVentas(fechaInicio, fechaFin);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<ResumenVentasDto>(okResult.Value);
        Assert.Equal(1000m, dto.TotalVentas);
        Assert.Equal(10, dto.CantidadReservas);
    }

    [Fact]
    public async Task ObtenerResumenVentas_ErrorRepositorio_Retorna500()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _controller.ObtenerResumenVentas(null, null);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task ObtenerResumenVentas_SinDatos_RetornaResumenVacio()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReporteVentasDiarias>());

        // Act
        var resultado = await _controller.ObtenerResumenVentas(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<ResumenVentasDto>(okResult.Value);
        Assert.Equal(0m, dto.TotalVentas);
        Assert.Equal(0, dto.CantidadReservas);
        Assert.Equal(0, dto.PromedioEvento);
    }

    #endregion

    #region ObtenerAsistenciaEvento Tests

    [Fact]
    public async Task ObtenerAsistenciaEvento_EventoNoExiste_Retorna404()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        _mockRepositorio.Setup(r => r.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync((HistorialAsistencia?)null);

        // Act
        var resultado = await _controller.ObtenerAsistenciaEvento(eventoId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task ObtenerAsistenciaEvento_EventoExiste_RetornaDatos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var historial = new HistorialAsistencia
        {
            EventoId = eventoId,
            TituloEvento = "Evento Test",
            TotalAsistentesRegistrados = 50,
            AsientosReservados = 30,
            AsientosDisponibles = 70,
            CapacidadTotal = 100,
            PorcentajeOcupacion = 30.0,
            UltimaActualizacion = DateTime.UtcNow
        };

        _mockRepositorio.Setup(r => r.ObtenerAsistenciaEventoAsync(eventoId))
            .ReturnsAsync(historial);

        // Act
        var resultado = await _controller.ObtenerAsistenciaEvento(eventoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<AsistenciaEventoDto>(okResult.Value);
        Assert.Equal(eventoId, dto.EventoId);
        Assert.Equal(50, dto.TotalAsistentes);
        Assert.Equal(30, dto.AsientosReservados);
        Assert.Equal(30.0, dto.PorcentajeOcupacion);
    }

    [Fact]
    public async Task ObtenerAsistenciaEvento_ErrorRepositorio_Retorna500()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        _mockRepositorio.Setup(r => r.ObtenerAsistenciaEventoAsync(eventoId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _controller.ObtenerAsistenciaEvento(eventoId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    #endregion

    #region ObtenerAuditoria Tests

    [Fact]
    public async Task ObtenerAuditoria_PaginaMenorQue1_Retorna400()
    {
        // Arrange
        var pagina = 0;

        // Act
        var resultado = await _controller.ObtenerAuditoria(null, null, null, pagina, 50);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task ObtenerAuditoria_TamañoPaginaInvalido_Retorna400()
    {
        // Arrange
        var tamañoPagina = 150; // Mayor que 100

        // Act
        var resultado = await _controller.ObtenerAuditoria(null, null, null, 1, tamañoPagina);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task ObtenerAuditoria_FechaInicioMayorQueFin_Retorna400()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow;
        var fechaFin = fechaInicio.AddDays(-1);

        // Act
        var resultado = await _controller.ObtenerAuditoria(fechaInicio, fechaFin, null, 1, 50);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task ObtenerAuditoria_ParametrosValidos_RetornaPaginacion()
    {
        // Arrange
        var logs = new List<LogAuditoria>
        {
            new LogAuditoria
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Evento",
                EntidadId = Guid.NewGuid().ToString(),
                Detalles = "Test",
                Exitoso = true
            }
        };

        _mockRepositorio.Setup(r => r.ObtenerLogsAuditoriaAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), 
            It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(logs);

        _mockRepositorio.Setup(r => r.ContarLogsAuditoriaAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _controller.ObtenerAuditoria(null, null, null, 1, 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<PaginacionDto<LogAuditoriaDto>>(okResult.Value);
        Assert.Single(dto.Datos);
        Assert.Equal(1, dto.PaginaActual);
        Assert.Equal(50, dto.TamañoPagina);
    }

    [Fact]
    public async Task ObtenerAuditoria_ErrorRepositorio_Retorna500()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerLogsAuditoriaAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), 
            It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _controller.ObtenerAuditoria(null, null, null, 1, 50);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    #endregion

    #region ObtenerConciliacionFinanciera Tests

    [Fact]
    public async Task ObtenerConciliacionFinanciera_FechaInicioMayorQueFin_Retorna400()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow;
        var fechaFin = fechaInicio.AddDays(-1);

        // Act
        var resultado = await _controller.ObtenerConciliacionFinanciera(fechaInicio, fechaFin);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task ObtenerConciliacionFinanciera_DatosValidos_RetornaConciliacion()
    {
        // Arrange
        var fechaInicio = DateTime.UtcNow.AddDays(-30);
        var fechaFin = DateTime.UtcNow;
        var ventas = new List<ReporteVentasDiarias>
        {
            new ReporteVentasDiarias
            {
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 1",
                Fecha = fechaInicio,
                CantidadReservas = 10,
                TotalIngresos = 1000m,
                ReservasPorCategoria = new Dictionary<string, int> 
                { 
                    { "VIP", 5 }, 
                    { "General", 5 } 
                }
            },
            new ReporteVentasDiarias
            {
                EventoId = Guid.NewGuid(),
                TituloEvento = "Evento 2",
                Fecha = fechaInicio.AddDays(1),
                CantidadReservas = 20,
                TotalIngresos = 2000m,
                ReservasPorCategoria = new Dictionary<string, int> 
                { 
                    { "VIP", 10 }, 
                    { "General", 10 } 
                }
            }
        };

        _mockRepositorio.Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventas);

        // Act
        var resultado = await _controller.ObtenerConciliacionFinanciera(fechaInicio, fechaFin);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<ConciliacionFinancieraDto>(okResult.Value);
        Assert.Equal(3000m, dto.TotalIngresos);
        Assert.Equal(30, dto.CantidadTransacciones);
        Assert.NotEmpty(dto.DesglosePorCategoria);
        Assert.Equal(2, dto.Transacciones.Count);
    }

    [Fact]
    public async Task ObtenerConciliacionFinanciera_SinDatos_RetornaConciliacionVacia()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReporteVentasDiarias>());

        // Act
        var resultado = await _controller.ObtenerConciliacionFinanciera(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<ConciliacionFinancieraDto>(okResult.Value);
        Assert.Equal(0m, dto.TotalIngresos);
        Assert.Equal(0, dto.CantidadTransacciones);
        Assert.Empty(dto.DesglosePorCategoria);
        Assert.Empty(dto.Transacciones);
    }

    [Fact]
    public async Task ObtenerConciliacionFinanciera_ErrorRepositorio_Retorna500()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerVentasPorRangoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _controller.ObtenerConciliacionFinanciera(null, null);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    #endregion

    #region ObtenerMetricasEvento Tests

    [Fact]
    public async Task ObtenerMetricasEvento_EventoNoExiste_Retorna404()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        _mockRepositorio.Setup(r => r.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync((MetricasEvento?)null);

        // Act
        var resultado = await _controller.ObtenerMetricasEvento(eventoId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        
        Assert.Contains("métricas", notFoundResult.Value?.ToString() ?? "");
    }

    [Fact]
    public async Task ObtenerMetricasEvento_EventoExiste_RetornaDatos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Evento Test",
            TotalAsistentes = 100,
            Estado = "Activo",
            FechaCreacion = DateTime.UtcNow.AddDays(-10),
            UltimaActualizacion = DateTime.UtcNow
        };

        _mockRepositorio.Setup(r => r.ObtenerMetricasEventoAsync(eventoId))
            .ReturnsAsync(metricas);

        // Act
        var resultado = await _controller.ObtenerMetricasEvento(eventoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<MetricasEventoDto>(okResult.Value);
        Assert.Equal(eventoId, dto.EventoId);
        Assert.Equal("Evento Test", dto.TituloEvento);
        Assert.Equal(100, dto.TotalAsistentes);
        Assert.Equal("Activo", dto.Estado);
        Assert.Equal(metricas.FechaCreacion, dto.FechaPublicacion);
        Assert.Equal(metricas.UltimaActualizacion, dto.UltimaActualizacion);
    }

    [Fact]
    public async Task ObtenerMetricasEvento_ErrorRepositorio_Retorna500()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        _mockRepositorio.Setup(r => r.ObtenerMetricasEventoAsync(eventoId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _controller.ObtenerMetricasEvento(eventoId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(500, statusResult.StatusCode);
        
        Assert.Contains("Error interno", statusResult.Value?.ToString() ?? "");
    }

    #endregion

    #region ObtenerLogsAuditoriaPorEvento Tests

    [Fact]
    public async Task ObtenerLogsAuditoriaPorEvento_SinEventoId_RetornaTodosLosLogs()
    {
        // Arrange
        var logs = new List<LogAuditoria>
        {
            new LogAuditoria
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Evento",
                EntidadId = Guid.NewGuid().ToString(),
                Detalles = "Test log 1",
                Exitoso = true,
                MensajeError = null
            },
            new LogAuditoria
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                TipoOperacion = "ReporteGenerado",
                Entidad = "Reporte",
                EntidadId = Guid.NewGuid().ToString(),
                Detalles = "Test log 2",
                Exitoso = false,
                MensajeError = "Error de prueba"
            }
        };

        _mockRepositorio.Setup(r => r.ObtenerLogsAuditoriaAsync(null, null, null, 1, 100))
            .ReturnsAsync(logs);

        // Act
        var resultado = await _controller.ObtenerLogsAuditoriaPorEvento(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dtos = Assert.IsType<List<LogAuditoriaDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Test log 1", dtos[0].Detalles);
        Assert.True(dtos[0].Exitoso);
        Assert.Null(dtos[0].MensajeError);
        Assert.Equal("Test log 2", dtos[1].Detalles);
        Assert.False(dtos[1].Exitoso);
        Assert.Equal("Error de prueba", dtos[1].MensajeError);
    }

    [Fact]
    public async Task ObtenerLogsAuditoriaPorEvento_ConEventoId_RetornaLogsFiltrados()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var logs = new List<LogAuditoria>
        {
            new LogAuditoria
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Evento",
                EntidadId = eventoId.ToString(),
                Detalles = "Log del evento específico",
                Exitoso = true,
                MensajeError = null
            },
            new LogAuditoria
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                TipoOperacion = "EventoConsumido",
                Entidad = "Evento",
                EntidadId = Guid.NewGuid().ToString(), // Diferente evento
                Detalles = "Log de otro evento",
                Exitoso = true,
                MensajeError = null
            }
        };

        _mockRepositorio.Setup(r => r.ObtenerLogsAuditoriaAsync(null, null, null, 1, 100))
            .ReturnsAsync(logs);

        // Act
        var resultado = await _controller.ObtenerLogsAuditoriaPorEvento(eventoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dtos = Assert.IsType<List<LogAuditoriaDto>>(okResult.Value);
        Assert.Single(dtos);
        Assert.Equal("Log del evento específico", dtos[0].Detalles);
        Assert.Equal(eventoId.ToString(), dtos[0].EntidadId);
    }

    [Fact]
    public async Task ObtenerLogsAuditoriaPorEvento_SinLogs_RetornaListaVacia()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerLogsAuditoriaAsync(null, null, null, 1, 100))
            .ReturnsAsync(new List<LogAuditoria>());

        // Act
        var resultado = await _controller.ObtenerLogsAuditoriaPorEvento(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var dtos = Assert.IsType<List<LogAuditoriaDto>>(okResult.Value);
        Assert.Empty(dtos);
    }

    [Fact]
    public async Task ObtenerLogsAuditoriaPorEvento_ErrorRepositorio_Retorna500()
    {
        // Arrange
        _mockRepositorio.Setup(r => r.ObtenerLogsAuditoriaAsync(null, null, null, 1, 100))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _controller.ObtenerLogsAuditoriaPorEvento(null);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(500, statusResult.StatusCode);
        
        Assert.Contains("Error interno", statusResult.Value?.ToString() ?? "");
    }

    #endregion
}
