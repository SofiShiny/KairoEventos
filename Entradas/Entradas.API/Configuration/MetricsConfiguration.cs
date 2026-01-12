using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Entradas.API.Configuration;

/// <summary>
/// Configuración de métricas y telemetría para el microservicio Entradas.API
/// </summary>
public static class MetricsConfiguration
{
    public const string MeterName = "Entradas.API";
    public const string ActivitySourceName = "Entradas.API";

    /// <summary>
    /// Configura las métricas y telemetría
    /// </summary>
    public static IServiceCollection AddMetricsConfiguration(this IServiceCollection services)
    {
        // Configurar métricas
        services.AddSingleton<Meter>(provider =>
        {
            return new Meter(MeterName, "1.0.0");
        });

        // Configurar ActivitySource para distributed tracing
        services.AddSingleton<ActivitySource>(provider =>
        {
            return new ActivitySource(ActivitySourceName, "1.0.0");
        });

        return services;
    }
}