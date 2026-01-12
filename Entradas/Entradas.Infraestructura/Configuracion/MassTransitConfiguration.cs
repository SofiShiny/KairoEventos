using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Entradas.Aplicacion.Consumers;

namespace Entradas.Infraestructura.Configuracion;

public static class MassTransitConfiguration
{
    public static IServiceCollection ConfigurarMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Configurar consumers
            x.AddConsumer<PagoConfirmadoConsumer>();
            x.AddConsumer<PagoAprobadoConsumer>();
            x.AddConsumer<AsientoLiberadoConsumer>();
            x.AddConsumer<PagoRechazadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                // ...
                var rabbitMqSettings = configuration.GetSection("RabbitMQ");
                var host = rabbitMqSettings["Host"] ?? "localhost";
                var port = rabbitMqSettings.GetValue<int>("Port", 5672);
                var username = rabbitMqSettings["Username"] ?? "guest";
                var password = rabbitMqSettings["Password"] ?? "guest";
                var virtualHost = rabbitMqSettings["VirtualHost"] ?? "/";

                cfg.Host(host, (ushort)port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configurar endpoints para consumers
                cfg.ReceiveEndpoint("entrada-pago-confirmado", e =>
                {
                    e.ConfigureConsumer<PagoConfirmadoConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("entrada-pago-aprobado", e =>
                {
                    e.ConfigureConsumer<PagoAprobadoConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("entrada-asiento-liberado", e =>
                {
                    e.ConfigureConsumer<AsientoLiberadoConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("entrada-pago-rechazado", e =>
                {
                    e.ConfigureConsumer<PagoRechazadoConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}