using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Marketing.Aplicacion.Interfaces;
using Marketing.Infraestructura.Persistencia;
using Marketing.Infraestructura.Persistencia.Repositorios;
using Marketing.Infraestructura.Adaptadores.Mensajeria;
using Marketing.Aplicacion.CasosUso;
using MassTransit;
using Marketing.Infraestructura.Adaptadores.Consumidores;

namespace Marketing.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistencia
        services.AddDbContext<MarketingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IRepositorioCupones, RepositorioCupones>();

        // Mensajería
        services.AddScoped<IEventoPublicador, EventoPublicador>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PagoAprobadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("marketing-pago-aprobado", e =>
                {
                    e.ConfigureConsumer<PagoAprobadoConsumer>(context);
                });
            });
        });

        // Casos de Uso
        services.AddScoped<CrearCuponUseCase>();
        services.AddScoped<EnviarCuponUseCase>();
        services.AddScoped<ValidarCuponUseCase>();
        services.AddScoped<ConsumirCuponUseCase>();

        return services;
    }
}
