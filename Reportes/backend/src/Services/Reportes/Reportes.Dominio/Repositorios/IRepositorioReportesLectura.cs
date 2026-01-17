using Reportes.Dominio.ModelosLectura;

namespace Reportes.Dominio.Repositorios;

public interface IRepositorioReportesLectura
{
    // Ventas
    Task<ReporteVentasDiarias?> ObtenerVentasDiariasAsync(DateTime fecha);
    Task ActualizarVentasDiariasAsync(ReporteVentasDiarias reporte);
    Task IncrementarVentasDiariasAsync(DateTime fecha, decimal monto, int cantidad);
    Task<List<ReporteVentasDiarias>> ObtenerVentasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    
    // Métricas Diarias
    Task<MetricasDiarias?> ObtenerMetricasDiariasAsync(DateTime fecha, Guid eventoId);
    Task ActualizarMetricasDiariasAsync(MetricasDiarias metricas);
    Task<List<MetricasDiarias>> ObtenerMetricasDiariasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    
    // Asistencia
    Task<HistorialAsistencia?> ObtenerAsistenciaEventoAsync(Guid eventoId);
    Task<List<HistorialAsistencia>> ObtenerTodoHistorialAsistenciaAsync();
    Task ActualizarAsistenciaAsync(HistorialAsistencia historial);
    
    // Métricas
    Task<MetricasEvento?> ObtenerMetricasEventoAsync(Guid eventoId);
    Task ActualizarMetricasAsync(MetricasEvento metricas);
    Task<List<MetricasEvento>> ObtenerTodasMetricasAsync();
    
    // Auditoría
    Task RegistrarLogAuditoriaAsync(LogAuditoria log);
    Task<List<LogAuditoria>> ObtenerLogsAuditoriaAsync(
        DateTime? fechaInicio, 
        DateTime? fechaFin, 
        string? tipoOperacion, 
        int pagina, 
        int tamañoPagina);
    Task<long> ContarLogsAuditoriaAsync(
        DateTime? fechaInicio, 
        DateTime? fechaFin, 
        string? tipoOperacion);
    
    // Consolidados
    Task<ReporteConsolidado?> ObtenerReporteConsolidadoAsync(DateTime fecha);
    Task GuardarReporteConsolidadoAsync(ReporteConsolidado reporte);
    Task<List<ReporteConsolidado>> ObtenerReportesConsolidadosPorRangoAsync(
        DateTime fechaInicio, 
        DateTime fechaFin);
}
