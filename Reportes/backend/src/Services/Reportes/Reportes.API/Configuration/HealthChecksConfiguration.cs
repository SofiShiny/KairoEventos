using Reportes.API.HealthChecks;

namespace Reportes.API.Configuration;

public static class HealthChecksConfiguration
{
    public static IServiceCollection ConfigureHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var enableHealthChecks = configuration.GetValue<bool>("HealthChecks:Enabled", true);
        var enableMassTransit = configuration.GetValue<bool>("MassTransit:Enabled", true);

        if (enableHealthChecks)
        {
            var healthChecksBuilder = services.AddHealthChecks()
                .AddCheck<MongoDbHealthCheck>("mongodb");
            
            // Solo agregar RabbitMQ health check si MassTransit está habilitado
            if (enableMassTransit)
            {
                healthChecksBuilder.AddCheck<RabbitMqHealthCheck>("rabbitmq");
            }
        }
            
        return services;
    }
}