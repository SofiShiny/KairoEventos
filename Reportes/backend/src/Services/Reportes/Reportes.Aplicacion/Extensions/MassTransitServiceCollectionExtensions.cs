using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reportes.Aplicacion.Configuracion;
using Reportes.Aplicacion.Consumers;

namespace Reportes.Aplicacion.Extensions;

/// <summary>
/// Extension methods for configuring MassTransit services
/// </summary>
public static class MassTransitServiceCollectionExtensions
{
    /// <summary>
    /// Configures MassTransit with RabbitMQ transport and consumers
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigurarMassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var enableMassTransit = configuration.GetValue<bool>("MassTransit:Enabled", true);
        
        if (enableMassTransit)
        {
            services.AddMassTransit(x =>
            {
                RegisterConsumers(x);
                ConfigureRabbitMq(x, configuration);
            });
        }
        else
        {
            RegisterConsumersAsServices(services);
        }

        return services;
    }

    /// <summary>
    /// Registers all event consumers with MassTransit
    /// </summary>
    private static void RegisterConsumers(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<EventoPublicadoConsumer>();
        configurator.AddConsumer<AsistenteRegistradoConsumer>();
        configurator.AddConsumer<EventoCanceladoConsumer>();
        configurator.AddConsumer<MapaAsientosCreadoConsumer>();
        configurator.AddConsumer<AsientoAgregadoConsumer>();
        configurator.AddConsumer<AsientoReservadoConsumer>();
        configurator.AddConsumer<AsientoLiberadoConsumer>();
        configurator.AddConsumer<EntradaCreadaConsumer>();
        configurator.AddConsumer<UsuarioAccionRealizadaConsumer>();
        configurator.AddConsumer<EntradaPagadaConsumer>();
        // configurator.AddConsumer<VentaRegistradaConsumer>(); // Reemplazado por EntradaPagadaConsumer
    }

    /// <summary>
    /// Registers consumers as regular services (for testing without MassTransit)
    /// </summary>
    private static void RegisterConsumersAsServices(IServiceCollection services)
    {
        services.AddScoped<EventoPublicadoConsumer>();
        services.AddScoped<AsistenteRegistradoConsumer>();
        services.AddScoped<EventoCanceladoConsumer>();
        services.AddScoped<MapaAsientosCreadoConsumer>();
        services.AddScoped<AsientoAgregadoConsumer>();
        services.AddScoped<AsientoReservadoConsumer>();
        services.AddScoped<AsientoLiberadoConsumer>();
        services.AddScoped<EntradaCreadaConsumer>();
        services.AddScoped<UsuarioAccionRealizadaConsumer>();
        services.AddScoped<EntradaPagadaConsumer>();
    }

    /// <summary>
    /// Configures RabbitMQ transport with connection settings and retry policies
    /// </summary>
    private static void ConfigureRabbitMq(
        IBusRegistrationConfigurator configurator,
        IConfiguration configuration)
    {
        configurator.UsingRabbitMq((context, cfg) =>
        {
            var connectionSettings = GetRabbitMqConnectionSettings(configuration);
            
            cfg.Host(connectionSettings.Host, connectionSettings.Port, "/", h =>
            {
                h.Username(connectionSettings.Username);
                h.Password(connectionSettings.Password);
            });

            ConfigureRetryPolicy(cfg);
            cfg.ConfigureEndpoints(context);
        });
    }

    /// <summary>
    /// Gets RabbitMQ connection settings from environment variables or configuration
    /// </summary>
    private static RabbitMqConnectionSettings GetRabbitMqConnectionSettings(IConfiguration configuration)
    {
        // Priorizar variables de ambiente directas o mapeadas por Docker
        var host = configuration["RabbitMQ:Host"] 
            ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
            ?? configuration["RabbitMqSettings:Host"] 
            ?? "localhost";
        
        var portStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") 
            ?? configuration["RabbitMqSettings:Port"]
            ?? configuration["RabbitMQ:Port"];

        var port = int.TryParse(portStr, out var portValue) 
                ? (ushort)portValue 
                : (ushort)5672;
        
        var username = Environment.GetEnvironmentVariable("RABBITMQ_USER") 
            ?? configuration["RabbitMqSettings:Username"] 
            ?? configuration["RabbitMQ:Username"]
            ?? "guest";
        
        var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") 
            ?? configuration["RabbitMqSettings:Password"] 
            ?? configuration["RabbitMQ:Password"]
            ?? "guest";

        return new RabbitMqConnectionSettings(host, port, username, password);
    }

    /// <summary>
    /// Configures exponential retry policy for message processing
    /// </summary>
    private static void ConfigureRetryPolicy(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(2),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromSeconds(2)
        ));
    }

    /// <summary>
    /// Represents RabbitMQ connection settings
    /// </summary>
    private record RabbitMqConnectionSettings(
        string Host,
        ushort Port,
        string Username,
        string Password);
}