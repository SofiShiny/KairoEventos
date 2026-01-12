using Microsoft.Extensions.DependencyInjection;
using Reportes.Aplicacion.Jobs;

namespace Reportes.Aplicacion.Extensions;

/// <summary>
/// Extension methods for configuring background jobs
/// </summary>
public static class JobsServiceCollectionExtensions
{
    /// <summary>
    /// Registers all background jobs with dependency injection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigurarJobs(this IServiceCollection services)
    {
        services.AddScoped<JobGenerarReportesConsolidados>();
        
        return services;
    }
}