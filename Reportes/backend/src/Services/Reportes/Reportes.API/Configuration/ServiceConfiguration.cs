using Reportes.Aplicacion;
using Reportes.Infraestructura;
using Serilog;

namespace Reportes.API.Configuration;

public static class ServiceConfiguration
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder
            .ConfigureLogging()
            .ConfigureUrls()
            .ConfigureApiServices()
            .ConfigureApplicationServices()
            .ConfigureInfrastructureServices()
            .ConfigureExternalServices();

        return builder;
    }

    private static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
            ?? builder.Configuration["MongoDbSettings:ConnectionString"] 
            ?? "mongodb://localhost:27017";
        var mongoDatabase = Environment.GetEnvironmentVariable("MONGODB_DATABASE") 
            ?? builder.Configuration["MongoDbSettings:DatabaseName"] 
            ?? "reportes_db";

        var mongoUrlForLogs = $"{mongoConnectionString}/{mongoDatabase}";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Microservicio", "Reportes")
            .Enrich.WithProperty("Ambiente", builder.Environment.EnvironmentName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.MongoDB(
                databaseUrl: mongoUrlForLogs,
                collectionName: "logs")
            .CreateLogger();

        builder.Host.UseSerilog();

        // Sobrescribir configuración con variables de entorno
        if (!string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            builder.Configuration["MongoDbSettings:ConnectionString"] = mongoConnectionString;
        }

        if (!string.IsNullOrWhiteSpace(mongoDatabase))
        {
            builder.Configuration["MongoDbSettings:DatabaseName"] = mongoDatabase;
        }

        return builder;
    }

    private static WebApplicationBuilder ConfigureUrls(this WebApplicationBuilder builder)
    {
        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:5002";
        builder.WebHost.UseUrls(urls);
        return builder;
    }

    private static WebApplicationBuilder ConfigureApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() 
            { 
                Title = "Reportes API", 
                Version = "v1",
                Description = "API de reportes y analíticas para el sistema de gestión de eventos. " +
                              "Proporciona endpoints para consultar resúmenes de ventas, asistencia a eventos, " +
                              "logs de auditoría y conciliación financiera.",
                Contact = new()
                {
                    Name = "Equipo de Desarrollo",
                    Email = "dev@eventos.com"
                }
            });
            
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
            
            options.EnableAnnotations();
            options.CustomSchemaIds(type => type.FullName);
        });

        // Real-Time Services
        builder.Services.AddSignalR();
        builder.Services.AddScoped<Reportes.Aplicacion.Interfaces.INotificadorDashboard, Reportes.API.Servicios.SignalRNotificadorDashboard>();
        builder.Services.AddScoped<Reportes.Aplicacion.Jobs.MetricasTiempoRealJob>();
        
        return builder;
    }

    private static WebApplicationBuilder ConfigureApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AgregarAplicacion(builder.Configuration);
        return builder;
    }

    private static WebApplicationBuilder ConfigureInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AgregarInfraestructura(builder.Configuration);
        return builder;
    }

    private static WebApplicationBuilder ConfigureExternalServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .ConfigureHealthChecks(builder.Configuration)
            .ConfigureCors();

        return builder;
    }
}