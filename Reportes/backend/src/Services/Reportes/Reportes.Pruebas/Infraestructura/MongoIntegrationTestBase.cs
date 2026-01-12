using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Reportes.Infraestructura.Configuracion;
using Reportes.Infraestructura.Persistencia;
using Reportes.Infraestructura.Repositorios;

namespace Reportes.Pruebas.Infraestructura;

/// <summary>
/// Clase base para tests de integración con MongoDB usando Mongo2Go.
/// Proporciona una instancia de MongoDB en memoria para tests rápidos y aislados.
/// </summary>
public abstract class MongoIntegrationTestBase : IDisposable
{
    protected MongoDbRunner MongoRunner { get; }
    protected ReportesMongoDbContext Context { get; }
    protected RepositorioReportesLecturaMongo Repositorio { get; }

    private static bool _conventionsRegistered = false;
    private static readonly object _lock = new object();

    protected MongoIntegrationTestBase()
    {
        // Configurar convenciones de MongoDB una sola vez
        ConfigureMongoDbConventions();

        // Iniciar MongoDB en memoria
        MongoRunner = MongoDbRunner.Start();

        // Configurar cliente con GuidRepresentation
        var settings = MongoClientSettings.FromConnectionString(MongoRunner.ConnectionString);
        settings.GuidRepresentation = GuidRepresentation.Standard;
        var client = new MongoClient(settings);

        // Obtener base de datos con nombre único para evitar conflictos entre tests
        var uniqueDbName = $"reportes_test_db_{Guid.NewGuid():N}";
        var database = client.GetDatabase(uniqueDbName);

        // Crear contexto con el constructor para tests
        var contextLogger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<ReportesMongoDbContext>();
        Context = new ReportesMongoDbContext(database, contextLogger);

        // Crear repositorio con logger
        var repoLogger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<RepositorioReportesLecturaMongo>();
        Repositorio = new RepositorioReportesLecturaMongo(Context, repoLogger);
    }

    private static void ConfigureMongoDbConventions()
    {
        lock (_lock)
        {
            if (_conventionsRegistered)
                return;

            // Registrar convenciones para camelCase
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camelCase", conventionPack, t => true);

            _conventionsRegistered = true;
        }
    }

    public void Dispose()
    {
        MongoRunner?.Dispose();
    }
}
