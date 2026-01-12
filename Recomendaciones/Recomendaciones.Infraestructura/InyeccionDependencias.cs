using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Recomendaciones.Dominio.Repositorios;
using Recomendaciones.Infraestructura.Persistencia;
using Recomendaciones.Infraestructura.Repositorios;
using Recomendaciones.Aplicacion.Consumers;

namespace Recomendaciones.Infraestructura;

public static class InyeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Persistencia
        services.AddDbContext<RecomendacionesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepositorioRecomendaciones, RepositorioRecomendaciones>();

        // 2. MassTransit - RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<EntradaCompradaConsumer>();
            x.AddConsumer<EventoCreadoConsumer>();
            x.AddConsumer<EventoCanceladoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("recomendaciones-eventos", e =>
                {
                    e.ConfigureConsumer<EntradaCompradaConsumer>(context);
                    e.ConfigureConsumer<EventoCreadoConsumer>(context);
                    e.ConfigureConsumer<EventoCanceladoConsumer>(context);
                });
            });
        });

        return services;
    }
}
