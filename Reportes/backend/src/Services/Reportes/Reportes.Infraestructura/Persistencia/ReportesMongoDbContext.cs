using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reportes.Dominio.ModelosLectura;
using Reportes.Infraestructura.Configuracion;

namespace Reportes.Infraestructura.Persistencia;

public class ReportesMongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public ReportesMongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
        
        // Crear índices al inicializar
        CrearIndices();
    }

    // Constructor adicional para tests con Mongo2Go
    public ReportesMongoDbContext(IMongoDatabase database, ILogger<ReportesMongoDbContext> logger)
    {
        _database = database;
        _settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = database.DatabaseNamespace.DatabaseName,
            ReportesVentasDiariasCollection = "reportes_ventas_diarias",
            HistorialAsistenciaCollection = "historial_asistencia",
            MetricasEventoCollection = "metricas_evento",
            LogsAuditoriaCollection = "logs_auditoria",
            ReportesConsolidadosCollection = "reportes_consolidados",
            TimelineCollection = "user_timeline",
            MetricasDiariasCollection = "metricas_diarias"
        };
        
        // Crear índices al inicializar
        CrearIndices();
    }

    // Constructor para tests con connection string directo
    public ReportesMongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _settings = new MongoDbSettings
        {
            ConnectionString = connectionString,
            DatabaseName = databaseName,
            ReportesVentasDiariasCollection = "reportes_ventas_diarias",
            HistorialAsistenciaCollection = "historial_asistencia",
            MetricasEventoCollection = "metricas_evento",
            LogsAuditoriaCollection = "logs_auditoria",
            ReportesConsolidadosCollection = "reportes_consolidados",
            TimelineCollection = "user_timeline",
            MetricasDiariasCollection = "metricas_diarias"
        };
        
        // Crear índices al inicializar
        CrearIndices();
    }

    // Exponer la base de datos para tests
    public IMongoDatabase Database => _database;

    public IMongoCollection<ReporteVentasDiarias> ReportesVentasDiarias =>
        _database.GetCollection<ReporteVentasDiarias>(_settings.ReportesVentasDiariasCollection);

    public IMongoCollection<HistorialAsistencia> HistorialAsistencia =>
        _database.GetCollection<HistorialAsistencia>(_settings.HistorialAsistenciaCollection);

    public IMongoCollection<MetricasEvento> MetricasEvento =>
        _database.GetCollection<MetricasEvento>(_settings.MetricasEventoCollection);

    public IMongoCollection<LogAuditoria> LogsAuditoria =>
        _database.GetCollection<LogAuditoria>(_settings.LogsAuditoriaCollection);

    public IMongoCollection<ReporteConsolidado> ReportesConsolidados =>
        _database.GetCollection<ReporteConsolidado>(_settings.ReportesConsolidadosCollection);

    public IMongoCollection<ElementoHistorial> Timeline =>
        _database.GetCollection<ElementoHistorial>(_settings.TimelineCollection);

    public IMongoCollection<MetricasDiarias> MetricasDiarias =>
        _database.GetCollection<MetricasDiarias>(_settings.MetricasDiariasCollection);

    private void CrearIndices()
    {
        // Índices para ReportesVentasDiarias
        var ventasIndexKeys = Builders<ReporteVentasDiarias>.IndexKeys
            .Ascending(r => r.Fecha)
            .Ascending(r => r.EventoId);
        var ventasIndexModel = new CreateIndexModel<ReporteVentasDiarias>(
            ventasIndexKeys,
            new CreateIndexOptions { Name = "idx_fecha_eventoId" });
        ReportesVentasDiarias.Indexes.CreateOne(ventasIndexModel);

        // Índices para HistorialAsistencia
        var asistenciaIndexKeys = Builders<HistorialAsistencia>.IndexKeys
            .Ascending(h => h.EventoId);
        var asistenciaIndexModel = new CreateIndexModel<HistorialAsistencia>(
            asistenciaIndexKeys,
            new CreateIndexOptions { Name = "idx_eventoId", Unique = true });
        HistorialAsistencia.Indexes.CreateOne(asistenciaIndexModel);

        // Índices para MetricasEvento
        var metricasIndexKeys = Builders<MetricasEvento>.IndexKeys
            .Ascending(m => m.EventoId);
        var metricasIndexModel = new CreateIndexModel<MetricasEvento>(
            metricasIndexKeys,
            new CreateIndexOptions { Name = "idx_eventoId", Unique = true });
        MetricasEvento.Indexes.CreateOne(metricasIndexModel);

        // Índices para LogsAuditoria
        var logsIndexKeys = Builders<LogAuditoria>.IndexKeys
            .Descending(l => l.Timestamp)
            .Ascending(l => l.TipoOperacion);
        var logsIndexModel = new CreateIndexModel<LogAuditoria>(
            logsIndexKeys,
            new CreateIndexOptions { Name = "idx_timestamp_tipoOperacion" });
        LogsAuditoria.Indexes.CreateOne(logsIndexModel);

        // Índices para ReportesConsolidados
        var consolidadosIndexKeys = Builders<ReporteConsolidado>.IndexKeys
            .Descending(r => r.FechaConsolidacion);
        var consolidadosIndexModel = new CreateIndexModel<ReporteConsolidado>(
            consolidadosIndexKeys,
            new CreateIndexOptions { Name = "idx_fechaConsolidacion" });
        ReportesConsolidados.Indexes.CreateOne(consolidadosIndexModel);

        // Índices para Timeline
        var timelineIndexKeys = Builders<ElementoHistorial>.IndexKeys
            .Ascending(t => t.UsuarioId)
            .Descending(t => t.Fecha);
        var timelineIndexModel = new CreateIndexModel<ElementoHistorial>(
            timelineIndexKeys,
            new CreateIndexOptions { Name = "idx_usuarioId_fecha" });
        Timeline.Indexes.CreateOne(timelineIndexModel);

        // Índices para MetricasDiarias
        var dailyMetricsIndexKeys = Builders<MetricasDiarias>.IndexKeys
            .Ascending(m => m.Fecha)
            .Ascending(m => m.EventoId);
        var dailyMetricsIndexModel = new CreateIndexModel<MetricasDiarias>(
            dailyMetricsIndexKeys,
            new CreateIndexOptions { Name = "idx_fecha_eventoId", Unique = true });
        MetricasDiarias.Indexes.CreateOne(dailyMetricsIndexModel);
    }

    public async Task<bool> VerificarConexionAsync()
    {
        try
        {
            await _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Limpia todas las colecciones de la base de datos para tests
    /// </summary>
    public async Task LimpiarBaseDatosAsync()
    {
        await ReportesVentasDiarias.DeleteManyAsync(Builders<ReporteVentasDiarias>.Filter.Empty);
        await HistorialAsistencia.DeleteManyAsync(Builders<HistorialAsistencia>.Filter.Empty);
        await MetricasEvento.DeleteManyAsync(Builders<MetricasEvento>.Filter.Empty);
        await LogsAuditoria.DeleteManyAsync(Builders<LogAuditoria>.Filter.Empty);
        await ReportesConsolidados.DeleteManyAsync(Builders<ReporteConsolidado>.Filter.Empty);
        await Timeline.DeleteManyAsync(Builders<ElementoHistorial>.Filter.Empty);
    }
}
