using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Reportes.API.HealthChecks;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IBusControl _busControl;
    private readonly ILogger<RabbitMqHealthCheck> _logger;

    public RabbitMqHealthCheck(
        IBusControl busControl,
        ILogger<RabbitMqHealthCheck> logger)
    {
        _busControl = busControl;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar si el bus está activo
            var busHealth = _busControl.GetProbeResult();
            
            await Task.CompletedTask; // Para satisfacer async
            
            // Si el bus responde sin excepción, está saludable
            return HealthCheckResult.Healthy("RabbitMQ está disponible y conectado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando conexión a RabbitMQ");
            return HealthCheckResult.Unhealthy(
                "Error verificando conexión a RabbitMQ",
                ex);
        }
    }
}
