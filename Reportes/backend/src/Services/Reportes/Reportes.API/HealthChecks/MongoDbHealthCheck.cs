using Microsoft.Extensions.Diagnostics.HealthChecks;
using Reportes.Infraestructura.Persistencia;

namespace Reportes.API.HealthChecks;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly ReportesMongoDbContext _context;

    public MongoDbHealthCheck(ReportesMongoDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conectado = await _context.VerificarConexionAsync();
            
            if (conectado)
            {
                return HealthCheckResult.Healthy("MongoDB está disponible");
            }
            
            return HealthCheckResult.Unhealthy("No se puede conectar a MongoDB");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Error verificando conexión a MongoDB", 
                ex);
        }
    }
}
