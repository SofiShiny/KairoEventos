using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Entradas.API.Configuration;

/// <summary>
/// Configuración de health checks para el microservicio Entradas.API
/// </summary>
public static class HealthChecksConfiguration
{
    /// <summary>
    /// Configura los health checks para PostgreSQL, RabbitMQ y servicios externos
    /// </summary>
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Health check básico de la aplicación
        healthChecksBuilder.AddCheck("self", () => HealthCheckResult.Healthy("API está funcionando correctamente"));

        // Health check para PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddNpgSql(
                connectionString,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "postgresql" },
                timeout: TimeSpan.FromSeconds(10));
        }

        // Health check para RabbitMQ
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rabbitMqConnectionString))
        {
            healthChecksBuilder.AddRabbitMQ(
                rabbitConnectionString: rabbitMqConnectionString,
                name: "rabbitmq",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "messaging", "rabbitmq" },
                timeout: TimeSpan.FromSeconds(10));
        }

        return services;
    }

    /// <summary>
    /// Configura los endpoints de health checks
    /// </summary>
    public static WebApplication UseHealthChecksConfiguration(this WebApplication app)
    {
        // Endpoint básico de health check
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Endpoint detallado de health check
        app.MapHealthChecks("/health/detailed", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Endpoint para health check de base de datos únicamente
        app.MapHealthChecks("/health/database", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database")
        });

        // Endpoint para health check de messaging únicamente
        app.MapHealthChecks("/health/messaging", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("messaging")
        });

        // Endpoint para health check de servicios externos únicamente
        app.MapHealthChecks("/health/external", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("external")
        });

        // Endpoint simple para load balancers (solo retorna 200 OK si está healthy)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Name == "self"
        });

        // Endpoint de readiness (incluye dependencias críticas)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database") || check.Tags.Contains("messaging") || check.Name == "self"
        });

        return app;
    }
}