using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using Entradas.Dominio.Interfaces;
using Entradas.Infraestructura.Persistencia;
using Entradas.Infraestructura.Repositorios;
using Entradas.Infraestructura.Servicios;
using Entradas.Infraestructura.ServiciosExternos;
using Entradas.Infraestructura.Configuracion;
using Hangfire;
using Hangfire.PostgreSql;

namespace Entradas.Infraestructura;

/// <summary>
/// Configuración de inyección de dependencias para la capa de infraestructura
/// </summary>
public static class InyeccionDependencias
{
    /// <summary>
    /// Registra los servicios de infraestructura en el contenedor de dependencias
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <returns>Colección de servicios configurada</returns>
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar Entity Framework Core con PostgreSQL
        services.AddDbContext<EntradasDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(EntradasDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
            
            // Configurar logging de EF Core en desarrollo
            var enableLogging = configuration.GetSection("Logging:EnableEntityFrameworkLogging").Get<bool>();
            if (enableLogging)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Registrar Unit of Work y repositorios
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepositorioEntradas, RepositorioEntradas>();

        // Registrar servicios de dominio
        services.AddScoped<IGeneradorCodigoQr, GeneradorCodigoQr>();

        // Registrar métricas
        services.AddSingleton<IEntradasMetrics>(provider =>
        {
            var meter = provider.GetRequiredService<System.Diagnostics.Metrics.Meter>();
            return new EntradasMetrics(meter);
        });

        // Configurar servicios HTTP externos
        services.ConfigurarServiciosExternos(configuration);

        // Configurar MassTransit para RabbitMQ
        services.ConfigurarMassTransit(configuration);

        // Configurar Hangfire
        services.ConfigurarHangfire(configuration);

        return services;
    }

    private static IServiceCollection ConfigurarHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => 
            {
                options.UseNpgsqlConnection(connectionString);
            }, new PostgreSqlStorageOptions
            {
                SchemaName = "hangfire_entradas",
                PrepareSchemaIfNecessary = true
            }));

        services.AddHangfireServer();

        return services;
    }

    /// <summary>
    /// Configura los servicios HTTP externos con políticas de resiliencia
    /// </summary>
    private static IServiceCollection ConfigurarServiciosExternos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar opciones para servicios externos
        services.Configure<VerificadorEventosOptions>(
            configuration.GetSection(VerificadorEventosOptions.SectionName));
        services.Configure<VerificadorAsientosOptions>(
            configuration.GetSection(VerificadorAsientosOptions.SectionName));

        // Configurar HttpClient para VerificadorEventos
        services.AddHttpClient<IVerificadorEventos, VerificadorEventosHttp>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VerificadorEventosOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Configurar HttpClient para VerificadorAsientos
        services.AddHttpClient<IVerificadorAsientos, VerificadorAsientosHttp>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VerificadorAsientosOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Configurar HttpClient para ServicioDescuentos
        services.AddHttpClient<IServicioDescuentos, ServicioDescuentosHttp>((serviceProvider, client) =>
        {
            var url = configuration["ServiciosExternos:MarketingUrl"] ?? "http://marketing-api:8080";
            client.BaseAddress = new Uri(url);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(GetRetryPolicy());

        // Configurar HttpClient para IAsientosService (Lead Architect Security Rule)
        services.AddHttpClient<IAsientosService, AsientosService>((serviceProvider, client) =>
        {
            // Usamos la configuración del microservicio de asientos
            var options = serviceProvider.GetRequiredService<IOptions<VerificadorAsientosOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    /// <summary>
    /// Crea una política de reintentos con backoff exponencial
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Reintento {retryCount} en {timespan.TotalMilliseconds}ms");
                });
    }

    /// <summary>
    /// Crea una política de circuit breaker
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Console.WriteLine($"Circuit breaker abierto por {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker cerrado");
                });
    }
}