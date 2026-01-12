using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using Reportes.Infraestructura.Persistencia;
using System.Collections.Generic;

namespace Reportes.Pruebas.API;

/// <summary>
/// Factory para crear instancias de la aplicación web para tests de integración.
/// Configura un entorno de test aislado con MongoDB en memoria y servicios mockeados.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private MongoDbRunner? _mongoRunner;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configurar entorno de test
        builder.UseEnvironment("Test");
        
        // Configurar configuración de test
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpiar configuración existente
            config.Sources.Clear();
            
            // Agregar configuración de test
            var testConfiguration = new Dictionary<string, string?>
            {
                ["MongoDbSettings:ConnectionString"] = GetMongoConnectionString(),
                ["MongoDbSettings:DatabaseName"] = "test_reportes_db",
                ["RabbitMqSettings:Host"] = "localhost",
                ["RabbitMqSettings:Username"] = "test",
                ["RabbitMqSettings:Password"] = "test",
                ["Hangfire:Enabled"] = "false", // Deshabilitar Hangfire en tests
                ["HealthChecks:Enabled"] = "true", // Mantener health checks habilitados para probar
                ["MassTransit:Enabled"] = "true", // Mantener MassTransit habilitado pero permitir fallos
                ["Logging:LogLevel:Default"] = "Warning", // Reducir logging en tests
                ["Logging:LogLevel:Microsoft"] = "Warning",
                ["Logging:LogLevel:System"] = "Warning"
            };
            
            config.AddInMemoryCollection(testConfiguration);
        });
        
        // Configurar servicios de test
        builder.ConfigureServices((context, services) =>
        {
            // Configurar logging para tests
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
        });
    }
    
    private string GetMongoConnectionString()
    {
        if (_mongoRunner == null)
        {
            _mongoRunner = MongoDbRunner.Start();
        }
        return _mongoRunner.ConnectionString;
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mongoRunner?.Dispose();
        }
        base.Dispose(disposing);
    }
    
    /// <summary>
    /// Obtiene el contexto de MongoDB configurado para tests
    /// </summary>
    public ReportesMongoDbContext GetMongoContext()
    {
        var connectionString = GetMongoConnectionString();
        var context = new ReportesMongoDbContext(connectionString, "test_reportes_db");
        return context;
    }
    
    /// <summary>
    /// Limpia la base de datos de test
    /// </summary>
    public async Task LimpiarBaseDatosAsync()
    {
        var context = GetMongoContext();
        await context.LimpiarBaseDatosAsync();
    }
}