using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Eventos.Dominio.Repositorios;
using Eventos.Infraestructura.Persistencia;
using Eventos.Infraestructura.Repositorios;

namespace Eventos.Infraestructura;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventoInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Lee variables de entorno para Docker y Azure
        // Permite cambiar la conexión sin recompilar el código
        var host = configuration["POSTGRES_HOST"];
        var db = configuration["POSTGRES_DB"];
        var user = configuration["POSTGRES_USER"];
        var password = configuration["POSTGRES_PASSWORD"];
        var port = configuration["POSTGRES_PORT"] ?? "5432";

        string connectionString;
        
        // el mismo código funciona en producción (Azure/Docker) y desarrollo local
        // Producción usa variables de entorno, desarrollo usa appsettings.json
        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(db))
        {
            connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
        }
        else
        {
            //para desarrollo local sin Docker
            connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("No se pudo configurar la cadena de conexión a la base de datos");
        }

        services.AddDbContext<EventosDbContext>(options =>
            options.UseNpgsql(connectionString));

        // cada request HTTP tiene su propia instancia del repositorio
        services.AddScoped<IRepositorioEvento, EventoRepository>();

        return services;
    }
}
