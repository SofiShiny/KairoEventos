using FluentAssertions;
using Reportes.Dominio.ModelosLectura;
using Xunit;

namespace Reportes.Pruebas.Infraestructura.Repositorios;

/// <summary>
/// Integration Tests para el repositorio de reportes de lectura usando MongoDB en memoria.
/// Valida: Requisitos 3.2, 3.4, 3.5
/// </summary>
public class RepositorioReportesLecturaIntegrationTests : MongoIntegrationTestBase
{
    #region Pruebas de Operaciones CRUD Básicas

    /// <summary>
    /// Test: Verificar que ActualizarMetricasAsync actualiza correctamente las métricas de un evento.
    /// Requisito: 3.2, 3.4
    /// </summary>
    [Fact]
    public async Task ActualizarMetricasAsync_DebeActualizarMetricasCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Evento de Prueba",
            FechaInicio = DateTime.UtcNow,
            Estado = "Publicado",
            TotalAsistentes = 100,
            TotalReservas = 80,
            IngresoTotal = 5000m
        };

        // Act
        await Repositorio.ActualizarMetricasAsync(metricas);
        var resultado = await Repositorio.ObtenerMetricasEventoAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        resultado.TituloEvento.Should().Be("Evento de Prueba");
        resultado.TotalAsistentes.Should().Be(100);
        resultado.TotalReservas.Should().Be(80);
        resultado.IngresoTotal.Should().Be(5000m);
        resultado.UltimaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Test: Verificar que RegistrarLogAuditoriaAsync inserta correctamente un log.
    /// Requisito: 3.2, 3.4
    /// </summary>
    [Fact]
    public async Task RegistrarLogAuditoriaAsync_DebeInsertarLogCorrectamente()
    {
        // Arrange
        var log = new LogAuditoria
        {
            TipoOperacion = "EventoConsumido",
            Entidad = "Evento",
            EntidadId = Guid.NewGuid().ToString(),
            Detalles = "Evento procesado exitosamente",
            Exitoso = true
        };

        // Act
        await Repositorio.RegistrarLogAuditoriaAsync(log);

        // Assert - Recuperar logs y verificar
        var logs = await Repositorio.ObtenerLogsAuditoriaAsync(null, null, null, 1, 10);
        logs.Should().NotBeEmpty();
        logs.Should().Contain(l => 
            l.TipoOperacion == "EventoConsumido" && 
            l.Entidad == "Evento" &&
            l.Exitoso == true);
    }

    /// <summary>
    /// Test: Verificar que ActualizarAsistenciaAsync actualiza el historial de asistencia.
    /// Requisito: 3.2, 3.4
    /// </summary>
    [Fact]
    public async Task ActualizarAsistenciaAsync_DebeActualizarHistorialCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var historial = new HistorialAsistencia
        {
            EventoId = eventoId,
            TituloEvento = "Evento de Prueba",
            CapacidadTotal = 100,
            AsientosReservados = 50,
            AsientosDisponibles = 50,
            TotalAsistentesRegistrados = 45,
            PorcentajeOcupacion = 50.0,
            Asistentes = new List<RegistroAsistente>()
        };

        // Act
        await Repositorio.ActualizarAsistenciaAsync(historial);
        var resultado = await Repositorio.ObtenerAsistenciaEventoAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        resultado.AsientosReservados.Should().Be(50);
        resultado.AsientosDisponibles.Should().Be(50);
        resultado.TotalAsistentesRegistrados.Should().Be(45);
        resultado.PorcentajeOcupacion.Should().Be(50.0);
    }

    /// <summary>
    /// Test: Verificar que ActualizarVentasDiariasAsync actualiza el reporte de ventas.
    /// Requisito: 3.2, 3.4
    /// </summary>
    [Fact]
    public async Task ActualizarVentasDiariasAsync_DebeActualizarReporteCorrectamente()
    {
        // Arrange
        var fecha = DateTime.UtcNow.Date;
        var eventoId = Guid.NewGuid();
        var reporte = new ReporteVentasDiarias
        {
            Fecha = fecha,
            EventoId = eventoId,
            TituloEvento = "Evento de Prueba",
            CantidadReservas = 50,
            TotalIngresos = 2500m,
            ReservasPorCategoria = new Dictionary<string, int>
            {
                { "VIP", 10 },
                { "General", 40 }
            }
        };

        // Act
        await Repositorio.ActualizarVentasDiariasAsync(reporte);
        var resultado = await Repositorio.ObtenerVentasDiariasAsync(fecha);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Fecha.Date.Should().Be(fecha);
        resultado.CantidadReservas.Should().Be(50);
        resultado.TotalIngresos.Should().Be(2500m);
        resultado.ReservasPorCategoria.Should().ContainKey("VIP");
        resultado.ReservasPorCategoria["VIP"].Should().Be(10);
    }

    #endregion

    #region Pruebas de Actualización de Registros Existentes

    /// <summary>
    /// Test: Verificar que se pueden actualizar métricas existentes.
    /// Requisito: 3.2, 3.4
    /// </summary>
    [Fact]
    public async Task ActualizarMetricasAsync_DebeActualizarRegistroExistente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var metricasIniciales = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Evento Inicial",
            FechaInicio = DateTime.UtcNow,
            Estado = "Publicado",
            TotalAsistentes = 50
        };

        await Repositorio.ActualizarMetricasAsync(metricasIniciales);
        var metricasExistentes = await Repositorio.ObtenerMetricasEventoAsync(eventoId);

        // Act - Actualizar con nuevos valores preservando el _id
        metricasExistentes!.TituloEvento = "Evento Actualizado";
        metricasExistentes.TotalAsistentes = 100;

        await Repositorio.ActualizarMetricasAsync(metricasExistentes);
        var resultado = await Repositorio.ObtenerMetricasEventoAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TituloEvento.Should().Be("Evento Actualizado");
        resultado.TotalAsistentes.Should().Be(100);
    }

    #endregion

    #region Pruebas de Consultas con Filtros

    /// <summary>
    /// Test: Verificar que se pueden obtener logs con filtros.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task ObtenerLogsAuditoriaAsync_DebeAplicarFiltrosCorrectamente()
    {
        // Arrange
        await Repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
        {
            TipoOperacion = "EventoConsumido",
            Entidad = "Evento",
            EntidadId = Guid.NewGuid().ToString(),
            Exitoso = true
        });

        await Repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
        {
            TipoOperacion = "ReporteGenerado",
            Entidad = "Reporte",
            EntidadId = Guid.NewGuid().ToString(),
            Exitoso = true
        });

        // Act - Filtrar por tipo de operación
        var logs = await Repositorio.ObtenerLogsAuditoriaAsync(
            null, null, "EventoConsumido", 1, 10);

        // Assert
        logs.Should().NotBeEmpty();
        logs.Should().OnlyContain(l => l.TipoOperacion == "EventoConsumido");
    }

    /// <summary>
    /// Test: Verificar que la paginación funciona correctamente.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task ObtenerLogsAuditoriaAsync_DebePaginarCorrectamente()
    {
        // Arrange - Insertar múltiples logs
        for (int i = 0; i < 15; i++)
        {
            await Repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                TipoOperacion = "Test",
                Entidad = "Test",
                EntidadId = i.ToString(),
                Exitoso = true
            });
        }

        // Act - Obtener primera página
        var pagina1 = await Repositorio.ObtenerLogsAuditoriaAsync(null, null, "Test", 1, 10);
        var pagina2 = await Repositorio.ObtenerLogsAuditoriaAsync(null, null, "Test", 2, 10);

        // Assert
        pagina1.Should().HaveCount(10);
        pagina2.Should().HaveCount(5);
    }

    #endregion

    #region Pruebas de Validación de Datos

    /// <summary>
    /// Test: Verificar que la UltimaActualizacion se establece automáticamente al actualizar métricas.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task ActualizarMetricasAsync_DebeEstablecerUltimaActualizacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Evento de Prueba",
            UltimaActualizacion = DateTime.UtcNow.AddDays(-1) // Fecha antigua
        };

        // Act
        await Repositorio.ActualizarMetricasAsync(metricas);
        var resultado = await Repositorio.ObtenerMetricasEventoAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.UltimaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Test: Verificar que el Timestamp se establece automáticamente al registrar log.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task RegistrarLogAuditoriaAsync_DebeEstablecerTimestamp()
    {
        // Arrange
        var log = new LogAuditoria
        {
            TipoOperacion = "EventoConsumido",
            Entidad = "Evento",
            EntidadId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.MinValue // Timestamp inválido
        };

        // Act
        await Repositorio.RegistrarLogAuditoriaAsync(log);
        var logs = await Repositorio.ObtenerLogsAuditoriaAsync(null, null, null, 1, 10);

        // Assert
        var logInsertado = logs.First();
        logInsertado.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Pruebas de Rendimiento

    /// <summary>
    /// Test: Verificar que las consultas se completan en menos de 500ms.
    /// Requisito: 3.3
    /// </summary>
    [Fact]
    public async Task ObtenerMetricasEventoAsync_DebeCompletarseEnMenosDe500ms()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        await Repositorio.ActualizarMetricasAsync(new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Test Performance",
            FechaInicio = DateTime.UtcNow,
            Estado = "Publicado"
        });

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var resultado = await Repositorio.ObtenerMetricasEventoAsync(eventoId);
        stopwatch.Stop();

        // Assert
        resultado.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    #endregion
}
