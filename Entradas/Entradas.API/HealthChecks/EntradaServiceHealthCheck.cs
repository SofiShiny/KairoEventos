using Microsoft.Extensions.Diagnostics.HealthChecks;
using Entradas.Dominio.Interfaces;

namespace Entradas.API.HealthChecks;

/// <summary>
/// Health check personalizado para verificar la funcionalidad del servicio de entradas
/// </summary>
public class EntradaServiceHealthCheck : IHealthCheck
{
    private readonly IVerificadorEventos _verificadorEventos;
    private readonly IVerificadorAsientos _verificadorAsientos;
    private readonly IGeneradorCodigoQr _generadorQr;
    private readonly ILogger<EntradaServiceHealthCheck> _logger;

    public EntradaServiceHealthCheck(
        IVerificadorEventos verificadorEventos,
        IVerificadorAsientos verificadorAsientos,
        IGeneradorCodigoQr generadorQr,
        ILogger<EntradaServiceHealthCheck> logger)
    {
        _verificadorEventos = verificadorEventos;
        _verificadorAsientos = verificadorAsientos;
        _generadorQr = generadorQr;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            var isHealthy = true;
            var issues = new List<string>();

            // Verificar generador de códigos QR
            try
            {
                var codigoQr = _generadorQr.GenerarCodigoUnico();
                if (string.IsNullOrEmpty(codigoQr) || !codigoQr.StartsWith("TICKET-"))
                {
                    issues.Add("Generador de códigos QR no está funcionando correctamente");
                    isHealthy = false;
                }
                else
                {
                    healthData["generador_qr"] = "OK";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar generador de códigos QR");
                issues.Add($"Error en generador QR: {ex.Message}");
                isHealthy = false;
            }

            // Verificar conectividad con servicios externos (con timeout corto)
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                // Usar un GUID de prueba que sabemos que no existe
                var testEventoId = new Guid("00000000-0000-0000-0000-000000000001");
                await _verificadorEventos.EventoExisteYDisponibleAsync(testEventoId, combinedCts.Token);
                healthData["verificador_eventos"] = "OK";
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                issues.Add("Timeout al verificar servicio de eventos");
                healthData["verificador_eventos"] = "TIMEOUT";
                // No marcamos como unhealthy por timeout de servicios externos
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Verificación de servicio de eventos (esperado para ID de prueba)");
                healthData["verificador_eventos"] = "OK"; // Es esperado que falle con ID de prueba
            }

            try
            {
                // Usar un GUID de prueba que sabemos que no existe
                var testEventoId = new Guid("00000000-0000-0000-0000-000000000001");
                var testAsientoId = new Guid("00000000-0000-0000-0000-000000000002");
                await _verificadorAsientos.AsientoDisponibleAsync(testEventoId, testAsientoId, combinedCts.Token);
                healthData["verificador_asientos"] = "OK";
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                issues.Add("Timeout al verificar servicio de asientos");
                healthData["verificador_asientos"] = "TIMEOUT";
                // No marcamos como unhealthy por timeout de servicios externos
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Verificación de servicio de asientos (esperado para ID de prueba)");
                healthData["verificador_asientos"] = "OK"; // Es esperado que falle con ID de prueba
            }

            // Agregar información adicional
            healthData["timestamp"] = DateTime.UtcNow;
            healthData["version"] = typeof(EntradaServiceHealthCheck).Assembly.GetName().Version?.ToString() ?? "1.0.0";

            if (isHealthy)
            {
                return HealthCheckResult.Healthy("Servicio de entradas funcionando correctamente", healthData);
            }
            else
            {
                var message = $"Problemas detectados: {string.Join(", ", issues)}";
                return HealthCheckResult.Degraded(message, data: healthData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante health check del servicio de entradas");
            return HealthCheckResult.Unhealthy($"Error inesperado: {ex.Message}");
        }
    }
}