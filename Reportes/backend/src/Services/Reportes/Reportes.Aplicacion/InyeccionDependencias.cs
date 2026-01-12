using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reportes.Aplicacion.Configuracion;
using Reportes.Aplicacion.Extensions;

namespace Reportes.Aplicacion;

/// <summary>
/// Extension methods for configuring application services
/// </summary>
public static class InyeccionDependencias
{
    /// <summary>
    /// Adds application services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AgregarAplicacion(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigurarConfiguracion(services, configuration);
        
        services.ConfigurarMassTransit(configuration);
        services.ConfigurarHangfire(configuration);
        services.ConfigurarJobs();
        
        return services;
    }

    /// <summary>
    /// Configures application configuration settings
    /// </summary>
    private static void ConfigurarConfiguracion(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(
            configuration.GetSection("RabbitMqSettings"));
    }
}
