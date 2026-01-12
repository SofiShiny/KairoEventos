using System.Net;
using System.Text.Json;
using Entradas.Dominio.Excepciones;
using FluentValidation;

namespace Entradas.API.Middleware;

/// <summary>
/// Middleware para el manejo global de excepciones en la aplicación
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no manejada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Title = "Error de validación",
                Detail = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                Status = (int)HttpStatusCode.BadRequest,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            EventoNoDisponibleException eventoEx => new ProblemDetails
            {
                Title = "Evento no disponible",
                Detail = eventoEx.Message,
                Status = (int)HttpStatusCode.NotFound,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            AsientoNoDisponibleException asientoEx => new ProblemDetails
            {
                Title = "Asiento no disponible",
                Detail = asientoEx.Message,
                Status = (int)HttpStatusCode.Conflict,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            },
            EntradaNoEncontradaException entradaEx => new ProblemDetails
            {
                Title = "Entrada no encontrada",
                Detail = entradaEx.Message,
                Status = (int)HttpStatusCode.NotFound,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            EstadoEntradaInvalidoException estadoEx => new ProblemDetails
            {
                Title = "Estado de entrada inválido",
                Detail = estadoEx.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            ServicioExternoNoDisponibleException servicioEx => new ProblemDetails
            {
                Title = "Servicio externo no disponible",
                Detail = "Uno de los servicios externos requeridos no está disponible. Intente nuevamente más tarde.",
                Status = (int)HttpStatusCode.ServiceUnavailable,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4"
            },
            DominioException dominioEx => new ProblemDetails
            {
                Title = "Error de dominio",
                Detail = dominioEx.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            ArgumentException argEx => new ProblemDetails
            {
                Title = "Argumento inválido",
                Detail = argEx.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            _ => new ProblemDetails
            {
                Title = "Error interno del servidor",
                Detail = "Ha ocurrido un error interno. Contacte al administrador del sistema.",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        // Agregar información adicional para debugging en desarrollo
        if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Agregar correlation ID si está disponible
        if (context.TraceIdentifier != null)
        {
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        }

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Clase para representar detalles del problema según RFC 7807
/// </summary>
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public Dictionary<string, object> Extensions { get; set; } = new();
}

/// <summary>
/// Extensiones para registrar el middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}