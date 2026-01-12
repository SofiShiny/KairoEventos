using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using Servicios.Dominio.Repositorios;
using Servicios.Infraestructura.Persistencia;
using Servicios.Infraestructura.Repositorios;
using Servicios.Infraestructura.ServiciosExternos;
using Servicios.Aplicacion.Consumers;

namespace Servicios.Infraestructura;

public static class InyeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Persistencia
        services.AddDbContext<ServiciosDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepositorioServicios, RepositorioServicios>();

        // 2. HTTP con Resiliencia (Polly)
        services.AddHttpClient<IVerificadorEntradas, VerificadorEntradasHttp>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:EntradasApiUrl"] ?? "http://entradas-api:8080");
        })
        .AddPolicyHandler(GetRetryPolicy());

        services.AddScoped<Dominio.Interfaces.IProveedorExternoService, MockProveedorService>();

        // 3. MassTransit - RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PagoAprobadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("servicios-pago-aprobado", e =>
                {
                    e.ConfigureConsumer<PagoAprobadoConsumer>(context);
                });
            });
        });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
