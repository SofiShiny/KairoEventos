using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reportes.Dominio.Repositorios;
using Reportes.Infraestructura.Configuracion;
using Reportes.Infraestructura.Persistencia;
using Reportes.Infraestructura.Repositorios;

namespace Reportes.Infraestructura;

public static class InyeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuración de MongoDB
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings"));

        // Registrar contexto de MongoDB
        services.AddSingleton<ReportesMongoDbContext>();

        // Registrar repositorios
        services.AddScoped<IRepositorioReportesLectura, RepositorioReportesLecturaMongo>();

        return services;
    }
}
