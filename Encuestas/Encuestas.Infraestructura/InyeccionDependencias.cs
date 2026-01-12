using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using Encuestas.Dominio.Repositorios;
using Encuestas.Infraestructura.Persistencia;
using Encuestas.Infraestructura.Repositorios;
using Encuestas.Infraestructura.ServiciosExternos;

namespace Encuestas.Infraestructura;

public static class InyeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Persistencia
        services.AddDbContext<EncuestasDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepositorioEncuestas, RepositorioEncuestas>();

        // 2. HTTP con Resiliencia (Polly)
        services.AddHttpClient<IVerificadorAsistencia, VerificadorAsistenciaHttp>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:EntradasApiUrl"] ?? "http://entradas-api:8080");
        })
        .AddPolicyHandler(GetRetryPolicy());

        // 3. MassTransit - RabbitMQ
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
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
