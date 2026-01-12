namespace Reportes.Infraestructura.Configuracion;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "reportes_db";
    
    // Nombres de colecciones
    public string ReportesVentasDiariasCollection { get; set; } = "reportes_ventas_diarias";
    public string HistorialAsistenciaCollection { get; set; } = "historial_asistencia";
    public string MetricasEventoCollection { get; set; } = "metricas_evento";
    public string LogsAuditoriaCollection { get; set; } = "logs_auditoria";
    public string ReportesConsolidadosCollection { get; set; } = "reportes_consolidados";
    public string TimelineCollection { get; set; } = "user_timeline";
    public string MetricasDiariasCollection { get; set; } = "metricas_diarias";
}
