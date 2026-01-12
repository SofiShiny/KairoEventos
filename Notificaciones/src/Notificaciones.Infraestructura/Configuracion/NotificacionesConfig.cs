using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notificaciones.Aplicacion.Consumers;
using Notificaciones.Dominio.Interfaces;
using Notificaciones.Infraestructura.Servicios;
using Microsoft.IdentityModel.Tokens;

namespace Notificaciones.Infraestructura.Configuracion;

public static class NotificacionesConfig
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConfiguracionEmail>(configuration.GetSection("Smtp"));
        services.AddScoped<IServicioEmail, ServicioEmailSmtp>();
        // services.AddScoped<INotificadorRealTime, NotificadorSignalR>();
        services.AddHttpClient<IServicioUsuarios, ServicioUsuariosHttp>();
        
        return services;
    }

    public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PagoAprobadoConsumer>();
            x.AddConsumer<CuponEnviadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("notificaciones-pago-aprobado", e =>
                {
                    e.ConfigureConsumer<PagoAprobadoConsumer>(context);
                });

                cfg.ReceiveEndpoint("notificaciones-cupon-enviado", e =>
                {
                    e.ConfigureConsumer<CuponEnviadoConsumer>(context);
                });
            });
        });

        return services;
    }

    public static void ConfigureJwtEvents(JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificacionesHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }
}
