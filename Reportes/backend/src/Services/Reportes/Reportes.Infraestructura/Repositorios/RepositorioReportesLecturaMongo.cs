using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using Reportes.Infraestructura.Persistencia;

namespace Reportes.Infraestructura.Repositorios;

public class RepositorioReportesLecturaMongo : IRepositorioReportesLectura
{
    private readonly ReportesMongoDbContext _context;
    private readonly ILogger<RepositorioReportesLecturaMongo> _logger;

    public RepositorioReportesLecturaMongo(
        ReportesMongoDbContext context,
        ILogger<RepositorioReportesLecturaMongo> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Ventas

    public async Task<ReporteVentasDiarias?> ObtenerVentasDiariasAsync(DateTime fecha)
    {
        try
        {
            var fechaSoloFecha = fecha.Date;
            var filter = Builders<ReporteVentasDiarias>.Filter.Eq(r => r.Fecha, fechaSoloFecha);
            return await _context.ReportesVentasDiarias.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo ventas diarias para fecha {Fecha}", fecha);
            throw;
        }
    }

    public async Task ActualizarVentasDiariasAsync(ReporteVentasDiarias reporte)
    {
        try
        {
            reporte.UltimaActualizacion = DateTime.UtcNow;
            
            var filter = Builders<ReporteVentasDiarias>.Filter.Eq(r => r.Fecha, reporte.Fecha);
            var options = new ReplaceOptions { IsUpsert = true };
            
            await _context.ReportesVentasDiarias.ReplaceOneAsync(filter, reporte, options);
            
            _logger.LogInformation("Reporte de ventas diarias actualizado para fecha {Fecha}", reporte.Fecha);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando ventas diarias para fecha {Fecha}", reporte.Fecha);
            throw;
        }
    }

    public async Task IncrementarVentasDiariasAsync(DateTime fecha, decimal monto, int cantidad)
    {
        try
        {
            var fechaSoloFecha = fecha.Date;
            var filter = Builders<ReporteVentasDiarias>.Filter.Eq(r => r.Fecha, fechaSoloFecha);
            
            var update = Builders<ReporteVentasDiarias>.Update
                .Inc(r => r.TotalIngresos, monto)
                .Inc(r => r.CantidadReservas, cantidad)
                .Set(r => r.UltimaActualizacion, DateTime.UtcNow)
                .SetOnInsert(r => r.TituloEvento, "Consolidado Diario")
                .SetOnInsert(r => r.EventoId, Guid.Empty)
                .SetOnInsert(r => r.ReservasPorCategoria, new Dictionary<string, int>()); // Ensure dictionary exists

            var options = new UpdateOptions { IsUpsert = true };
            
            await _context.ReportesVentasDiarias.UpdateOneAsync(filter, update, options);
            _logger.LogInformation("Ventas incrementadas atomicamente fecha {Fecha}: +{Monto}", fechaSoloFecha, monto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementando ventas diarias atomicamente");
            throw;
        }
    }

    public async Task<List<ReporteVentasDiarias>> ObtenerVentasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var filter = Builders<ReporteVentasDiarias>.Filter.And(
                Builders<ReporteVentasDiarias>.Filter.Gte(r => r.Fecha, fechaInicio.Date),
                Builders<ReporteVentasDiarias>.Filter.Lte(r => r.Fecha, fechaFin.Date)
            );
            
            return await _context.ReportesVentasDiarias
                .Find(filter)
                .SortBy(r => r.Fecha)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo ventas por rango {FechaInicio} - {FechaFin}", 
                fechaInicio, fechaFin);
            throw;
        }
    }

    // Métricas Diarias
    public async Task<MetricasDiarias?> ObtenerMetricasDiariasAsync(DateTime fecha, Guid eventoId)
    {
        try
        {
            var fechaSoloFecha = fecha.Date;
            var filter = Builders<MetricasDiarias>.Filter.And(
                Builders<MetricasDiarias>.Filter.Eq(r => r.Fecha, fechaSoloFecha),
                Builders<MetricasDiarias>.Filter.Eq(r => r.EventoId, eventoId)
            );
            return await _context.MetricasDiarias.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas diarias para fecha {Fecha} y evento {EventoId}", fecha, eventoId);
            throw;
        }
    }

    public async Task ActualizarMetricasDiariasAsync(MetricasDiarias metricas)
    {
        try
        {
            var filter = Builders<MetricasDiarias>.Filter.And(
                Builders<MetricasDiarias>.Filter.Eq(r => r.Fecha, metricas.Fecha.Date),
                Builders<MetricasDiarias>.Filter.Eq(r => r.EventoId, metricas.EventoId)
            );
            var options = new ReplaceOptions { IsUpsert = true };
            await _context.MetricasDiarias.ReplaceOneAsync(filter, metricas, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando métricas diarias para fecha {Fecha}", metricas.Fecha);
            throw;
        }
    }

    public async Task<List<MetricasDiarias>> ObtenerMetricasDiariasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1);

            var filter = Builders<MetricasDiarias>.Filter.And(
                Builders<MetricasDiarias>.Filter.Gte(r => r.Fecha, inicio),
                Builders<MetricasDiarias>.Filter.Lt(r => r.Fecha, fin)
            );
            return await _context.MetricasDiarias.Find(filter).SortBy(r => r.Fecha).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas diarias por rango");
            throw;
        }
    }

    #endregion

    #region Asistencia

    public async Task<HistorialAsistencia?> ObtenerAsistenciaEventoAsync(Guid eventoId)
    {
        try
        {
            var filter = Builders<HistorialAsistencia>.Filter.Eq(h => h.EventoId, eventoId);
            return await _context.HistorialAsistencia.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo asistencia para evento {EventoId}", eventoId);
            throw;
        }
    }

    public async Task<List<HistorialAsistencia>> ObtenerTodoHistorialAsistenciaAsync()
    {
        try
        {
            return await _context.HistorialAsistencia.Find(Builders<HistorialAsistencia>.Filter.Empty).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todo el historial de asistencia");
            throw;
        }
    }

    public async Task ActualizarAsistenciaAsync(HistorialAsistencia historial)
    {
        try
        {
            historial.UltimaActualizacion = DateTime.UtcNow;
            
            var filter = Builders<HistorialAsistencia>.Filter.Eq(h => h.EventoId, historial.EventoId);
            var options = new ReplaceOptions { IsUpsert = true };
            
            await _context.HistorialAsistencia.ReplaceOneAsync(filter, historial, options);
            
            _logger.LogInformation("Historial de asistencia actualizado para evento {EventoId}", 
                historial.EventoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando asistencia para evento {EventoId}", 
                historial.EventoId);
            throw;
        }
    }

    #endregion

    #region Métricas

    public async Task<MetricasEvento?> ObtenerMetricasEventoAsync(Guid eventoId)
    {
        try
        {
            var filter = Builders<MetricasEvento>.Filter.Eq(m => m.EventoId, eventoId);
            return await _context.MetricasEvento.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas para evento {EventoId}", eventoId);
            throw;
        }
    }

    public async Task ActualizarMetricasAsync(MetricasEvento metricas)
    {
        try
        {
            metricas.UltimaActualizacion = DateTime.UtcNow;
            
            var filter = Builders<MetricasEvento>.Filter.Eq(m => m.EventoId, metricas.EventoId);
            var options = new ReplaceOptions { IsUpsert = true };
            
            await _context.MetricasEvento.ReplaceOneAsync(filter, metricas, options);
            
            _logger.LogInformation("Métricas actualizadas para evento {EventoId}", metricas.EventoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando métricas para evento {EventoId}", 
                metricas.EventoId);
            throw;
        }
    }

    public async Task<List<MetricasEvento>> ObtenerTodasMetricasAsync()
    {
        try
        {
            return await _context.MetricasEvento
                .Find(Builders<MetricasEvento>.Filter.Empty)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todas las métricas");
            throw;
        }
    }

    #endregion

    #region Auditoría

    public async Task RegistrarLogAuditoriaAsync(LogAuditoria log)
    {
        try
        {
            log.Timestamp = DateTime.UtcNow;
            await _context.LogsAuditoria.InsertOneAsync(log);
            
            _logger.LogDebug("Log de auditoría registrado: {TipoOperacion} - {Entidad}", 
                log.TipoOperacion, log.Entidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando log de auditoría");
            throw;
        }
    }

    public async Task<List<LogAuditoria>> ObtenerLogsAuditoriaAsync(
        DateTime? fechaInicio,
        DateTime? fechaFin,
        string? tipoOperacion,
        int pagina,
        int tamañoPagina)
    {
        try
        {
            var filterBuilder = Builders<LogAuditoria>.Filter;
            var filters = new List<FilterDefinition<LogAuditoria>>();

            if (fechaInicio.HasValue)
                filters.Add(filterBuilder.Gte(l => l.Timestamp, fechaInicio.Value));

            if (fechaFin.HasValue)
                filters.Add(filterBuilder.Lte(l => l.Timestamp, fechaFin.Value));

            if (!string.IsNullOrWhiteSpace(tipoOperacion))
                filters.Add(filterBuilder.Eq(l => l.TipoOperacion, tipoOperacion));

            var filter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            var skip = (pagina - 1) * tamañoPagina;

            return await _context.LogsAuditoria
                .Find(filter)
                .SortByDescending(l => l.Timestamp)
                .Skip(skip)
                .Limit(tamañoPagina)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo logs de auditoría");
            throw;
        }
    }

    public async Task<long> ContarLogsAuditoriaAsync(
        DateTime? fechaInicio,
        DateTime? fechaFin,
        string? tipoOperacion)
    {
        try
        {
            var filterBuilder = Builders<LogAuditoria>.Filter;
            var filters = new List<FilterDefinition<LogAuditoria>>();

            if (fechaInicio.HasValue)
                filters.Add(filterBuilder.Gte(l => l.Timestamp, fechaInicio.Value));

            if (fechaFin.HasValue)
                filters.Add(filterBuilder.Lte(l => l.Timestamp, fechaFin.Value));

            if (!string.IsNullOrWhiteSpace(tipoOperacion))
                filters.Add(filterBuilder.Eq(l => l.TipoOperacion, tipoOperacion));

            var filter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _context.LogsAuditoria.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error contando logs de auditoría");
            throw;
        }
    }

    #endregion

    #region Consolidados

    public async Task<ReporteConsolidado?> ObtenerReporteConsolidadoAsync(DateTime fecha)
    {
        try
        {
            var fechaSoloFecha = fecha.Date;
            var filter = Builders<ReporteConsolidado>.Filter.And(
                Builders<ReporteConsolidado>.Filter.Gte(r => r.PeriodoInicio, fechaSoloFecha),
                Builders<ReporteConsolidado>.Filter.Lte(r => r.PeriodoFin, fechaSoloFecha.AddDays(1))
            );
            
            return await _context.ReportesConsolidados.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo reporte consolidado para fecha {Fecha}", fecha);
            throw;
        }
    }

    public async Task GuardarReporteConsolidadoAsync(ReporteConsolidado reporte)
    {
        try
        {
            reporte.FechaConsolidacion = DateTime.UtcNow;
            await _context.ReportesConsolidados.InsertOneAsync(reporte);
            
            _logger.LogInformation("Reporte consolidado guardado para período {PeriodoInicio} - {PeriodoFin}",
                reporte.PeriodoInicio, reporte.PeriodoFin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando reporte consolidado");
            throw;
        }
    }

    public async Task<List<ReporteConsolidado>> ObtenerReportesConsolidadosPorRangoAsync(
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        try
        {
            var filter = Builders<ReporteConsolidado>.Filter.And(
                Builders<ReporteConsolidado>.Filter.Gte(r => r.PeriodoInicio, fechaInicio),
                Builders<ReporteConsolidado>.Filter.Lte(r => r.PeriodoFin, fechaFin)
            );
            
            return await _context.ReportesConsolidados
                .Find(filter)
                .SortByDescending(r => r.FechaConsolidacion)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo reportes consolidados por rango");
            throw;
        }
    }

    #endregion
}
