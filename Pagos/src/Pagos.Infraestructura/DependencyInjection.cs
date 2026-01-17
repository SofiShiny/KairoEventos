using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pagos.Dominio.Interfaces;
using Pagos.Infraestructura.Persistencia;
using Pagos.Infraestructura.Pasarela;
using Pagos.Infraestructura.Facturacion;
using Pagos.Infraestructura.Almacenamiento;
using Pagos.Aplicacion.CasosUso;
using Hangfire;
using Hangfire.PostgreSql;

namespace Pagos.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        // EF Core
        services.AddDbContext<PagosDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositorios
        services.AddScoped<IRepositorioTransacciones, RepositorioTransacciones>();
        services.AddScoped<ICuponRepositorio, Pagos.Infraestructura.Repositorios.CuponRepositorio>();

        // Puertos
        services.AddScoped<IPasarelaPago, SimuladorPasarela>();
        services.AddScoped<IGeneradorFactura, GeneradorFacturaQuestPdf>();
        services.AddScoped<IAlmacenadorArchivos, AlmacenadorLocal>();

        // Casos de Uso
        services.AddScoped<IProcesarPagoUseCase, ProcesarPagoUseCase>();
        
        // Servicios
        services.AddScoped<Pagos.Aplicacion.Servicios.ICuponServicio, Pagos.Aplicacion.Servicios.CuponServicio>();

        // Hangfire
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });
        services.AddHangfireServer();

        // Registrar Jobs para Hangfire
        services.AddScoped<Pagos.Aplicacion.Jobs.ConciliacionJob>();
        services.AddScoped<Pagos.Aplicacion.Jobs.ConciliacionPagosJob>();

        return services;
    }
}
