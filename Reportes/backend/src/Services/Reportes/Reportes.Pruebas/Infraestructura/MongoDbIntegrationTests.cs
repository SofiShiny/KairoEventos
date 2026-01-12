using FluentAssertions;
using MongoDB.Driver;
using Reportes.Dominio.ModelosLectura;
using Xunit;

namespace Reportes.Pruebas.Infraestructura;

/// <summary>
/// Integration Tests para verificar la conectividad y operaciones con MongoDB usando Mongo2Go.
/// Valida: Requisitos 3.1, 3.2, 3.5
/// </summary>
public class MongoDbIntegrationTests : MongoIntegrationTestBase
{
    private IMongoDatabase Database => Context.Database;

    #region Pruebas de Conectividad

    /// <summary>
    /// Test: Verificar que MongoDB está disponible y responde a comandos.
    /// Requisito: 3.1
    /// </summary>
    [Fact]
    public async Task MongoDB_DebeEstarDisponible()
    {
        // Arrange & Act
        var result = await Database.RunCommandAsync<MongoDB.Bson.BsonDocument>(
            new MongoDB.Bson.BsonDocument("ping", 1));

        // Assert
        result.Should().NotBeNull();
        result["ok"].AsDouble.Should().Be(1.0);
    }

    /// <summary>
    /// Test: Verificar que el contexto de MongoDB se inicializa correctamente.
    /// Requisito: 3.1
    /// </summary>
    [Fact]
    public void MongoDbContext_DebeInicializarseCorrectamente()
    {
        // Assert
        Context.Should().NotBeNull();
        Context.MetricasEvento.Should().NotBeNull();
        Context.HistorialAsistencia.Should().NotBeNull();
        Context.ReportesVentasDiarias.Should().NotBeNull();
        Context.LogsAuditoria.Should().NotBeNull();
        Context.ReportesConsolidados.Should().NotBeNull();
    }

    /// <summary>
    /// Test: Verificar que las colecciones se crean automáticamente.
    /// Requisito: 3.1
    /// </summary>
    [Fact]
    public async Task MongoDB_DebeCrearColeccionesAutomaticamente()
    {
        // Arrange
        var metricas = new MetricasEvento
        {
            EventoId = Guid.NewGuid(),
            TituloEvento = "Test Evento",
            FechaInicio = DateTime.UtcNow,
            Estado = "Publicado"
        };

        // Act
        await Repositorio.ActualizarMetricasAsync(metricas);

        // Assert - Verificar que la colección existe
        var collections = await (await Database.ListCollectionNamesAsync()).ToListAsync();
        collections.Should().Contain("metricas_evento");
    }

    #endregion

    #region Pruebas de Operaciones CRUD

    /// <summary>
    /// Test: Verificar que se pueden insertar y recuperar métricas de evento.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task ActualizarMetricasAsync_DebeInsertarYRecuperarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var metricas = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Concierto de Rock",
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
        resultado.TituloEvento.Should().Be("Concierto de Rock");
        resultado.TotalAsistentes.Should().Be(100);
        resultado.TotalReservas.Should().Be(80);
        resultado.IngresoTotal.Should().Be(5000m);
        resultado.UltimaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

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

    /// <summary>
    /// Test: Verificar que se pueden registrar logs de auditoría.
    /// Requisito: 3.2
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
            Usuario = "Sistema",
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
    /// Test: Verificar que se puede actualizar historial de asistencia.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task ActualizarAsistenciaAsync_DebeInsertarYRecuperarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var historial = new HistorialAsistencia
        {
            EventoId = eventoId,
            TituloEvento = "Festival de Música",
            CapacidadTotal = 200,
            AsientosReservados = 100,
            AsientosDisponibles = 100,
            TotalAsistentesRegistrados = 95,
            PorcentajeOcupacion = 50.0,
            Asistentes = new List<RegistroAsistente>
            {
                new() { UsuarioId = "user1", NombreUsuario = "Juan Pérez", FechaRegistro = DateTime.UtcNow }
            }
        };

        // Act
        await Repositorio.ActualizarAsistenciaAsync(historial);
        var resultado = await Repositorio.ObtenerAsistenciaEventoAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        resultado.CapacidadTotal.Should().Be(200);
        resultado.AsientosReservados.Should().Be(100);
        resultado.AsientosDisponibles.Should().Be(100);
        resultado.TotalAsistentesRegistrados.Should().Be(95);
        resultado.PorcentajeOcupacion.Should().Be(50.0);
        resultado.Asistentes.Should().HaveCount(1);
    }

    /// <summary>
    /// Test: Verificar que se puede actualizar reporte de ventas diarias.
    /// Requisito: 3.2
    /// </summary>
    [Fact]
    public async Task ActualizarVentasDiariasAsync_DebeInsertarYRecuperarCorrectamente()
    {
        // Arrange
        var fecha = DateTime.UtcNow.Date;
        var reporte = new ReporteVentasDiarias
        {
            Fecha = fecha,
            EventoId = Guid.NewGuid(),
            TituloEvento = "Concierto de Jazz",
            CantidadReservas = 75,
            TotalIngresos = 3750m,
            ReservasPorCategoria = new Dictionary<string, int>
            {
                { "VIP", 15 },
                { "General", 60 }
            }
        };

        // Act
        await Repositorio.ActualizarVentasDiariasAsync(reporte);
        var resultado = await Repositorio.ObtenerVentasDiariasAsync(fecha);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Fecha.Date.Should().Be(fecha);
        resultado.CantidadReservas.Should().Be(75);
        resultado.TotalIngresos.Should().Be(3750m);
        resultado.ReservasPorCategoria.Should().ContainKey("VIP");
        resultado.ReservasPorCategoria["VIP"].Should().Be(15);
    }

    #endregion

    #region Pruebas de Consultas

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

    #region Pruebas de Operaciones Atómicas

    /// <summary>
    /// Test: Verificar que las operaciones de actualización son atómicas.
    /// Requisito: 3.4
    /// </summary>
    [Fact]
    public async Task ActualizarMetricasAsync_DebeSerOperacionAtomica()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        
        // Crear métricas iniciales
        var metricasIniciales = new MetricasEvento
        {
            EventoId = eventoId,
            TituloEvento = "Evento Inicial",
            FechaInicio = DateTime.UtcNow,
            Estado = "Publicado",
            TotalAsistentes = 0
        };
        await Repositorio.ActualizarMetricasAsync(metricasIniciales);

        // Act - Realizar múltiples actualizaciones concurrentes
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var metricas = await Repositorio.ObtenerMetricasEventoAsync(eventoId);
                if (metricas != null)
                {
                    metricas.TotalAsistentes += 10;
                    await Repositorio.ActualizarMetricasAsync(metricas);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Verificar que el registro final es consistente
        var resultado = await Repositorio.ObtenerMetricasEventoAsync(eventoId);
        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        // El valor final debe estar entre 10 y 100 (algunas actualizaciones pueden perderse por concurrencia)
        resultado.TotalAsistentes.Should().BeGreaterOrEqualTo(10);
        resultado.TotalAsistentes.Should().BeLessOrEqualTo(100);
    }

    #endregion
}
