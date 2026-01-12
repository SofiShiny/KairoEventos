using System.Diagnostics;
using Entradas.Dominio.Interfaces;

namespace Entradas.API.Middleware;

/// <summary>
/// Middleware para recopilar métricas de performance de requests HTTP
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;
    private readonly IEntradasMetrics _metrics;
    private readonly ActivitySource _activitySource;

    public MetricsMiddleware(
        RequestDelegate next,
        ILogger<MetricsMiddleware> logger,
        IEntradasMetrics metrics,
        ActivitySource activitySource)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        // Crear actividad para distributed tracing
        using var activity = _activitySource.StartActivity($"{method} {path}");
        
        // Agregar tags a la actividad
        activity?.SetTag("http.method", method);
        activity?.SetTag("http.path", path);
        activity?.SetTag("http.scheme", context.Request.Scheme);
        activity?.SetTag("http.host", context.Request.Host.Value);

        // Agregar correlation ID si existe
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            activity?.SetTag("correlation.id", correlationId.FirstOrDefault());
        }

        try
        {
            await _next(context);

            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalMilliseconds;

            // Registrar métricas de performance
            LogRequestMetrics(context, duration, "success");

            // Agregar información a la actividad
            activity?.SetTag("http.status_code", context.Response.StatusCode);
            activity?.SetTag("http.response_time_ms", duration);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else if (context.Response.StatusCode >= 400)
            {
                activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {context.Response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalMilliseconds;

            // Registrar métricas de error
            LogRequestMetrics(context, duration, "error");

            // Agregar información de error a la actividad
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);

            _logger.LogError(ex, "Error no manejado en request {Method} {Path} después de {Duration}ms",
                method, path, duration);

            throw;
        }
    }

    private void LogRequestMetrics(HttpContext context, double durationMs, string resultado)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;

        // Solo registrar métricas para endpoints de la API (no para health checks, swagger, etc.)
        if (path.StartsWith("/api/"))
        {
            _logger.LogDebug("Request {Method} {Path} completado en {Duration}ms con status {StatusCode}",
                method, path, durationMs, statusCode);

            // Aquí podrías agregar métricas más específicas según el endpoint
            if (path.StartsWith("/api/entradas") && method == "POST")
            {
                // Métricas específicas para creación de entradas se manejan en el handler
            }
        }

        // Registrar métricas generales de health checks
        if (path.StartsWith("/health"))
        {
            var checkName = path.Replace("/health", "").TrimStart('/');
            if (string.IsNullOrEmpty(checkName))
            {
                checkName = "general";
            }

            var healthResult = statusCode == 200 ? "healthy" : 
                              statusCode == 503 ? "unhealthy" : "degraded";

            _metrics.RecordHealthCheckResult(checkName, healthResult, durationMs);
        }
    }
}

/// <summary>
/// Extensiones para registrar el middleware de métricas
/// </summary>
public static class MetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MetricsMiddleware>();
    }
}