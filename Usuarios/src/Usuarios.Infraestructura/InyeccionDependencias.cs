using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;
using Usuarios.Infraestructura.Persistencia;
using Usuarios.Infraestructura.Repositorios;
using Usuarios.Infraestructura.Servicios;

namespace Usuarios.Infraestructura
{
    public static class InyeccionDependencias
    {
        public static IServiceCollection AgregarInfraestructura(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar DbContext con PostgreSQL
            var connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new InvalidOperationException("La cadena de conexión 'PostgresConnection' no está configurada");
            
            services.AddDbContext<UsuariosDbContext>(options =>
                options.UseNpgsql(connectionString));
            
            // Registrar Repositorios
            services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
            
            // Registrar HttpClient para ServicioKeycloak
            services.AddHttpClient<IServicioKeycloak, ServicioKeycloak>();
            
            // Configurar MassTransit para Auditoría
            services.ConfigurarMassTransit(configuration);

            return services;
        }

        private static IServiceCollection ConfigurarMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqSettings = configuration.GetSection("RabbitMQ");
                    cfg.Host(rabbitMqSettings["Host"] ?? "localhost", h =>
                    {
                        h.Username(rabbitMqSettings["Username"] ?? "guest");
                        h.Password(rabbitMqSettings["Password"] ?? "guest");
                    });
                });
            });

            return services;
        }
    }
}
