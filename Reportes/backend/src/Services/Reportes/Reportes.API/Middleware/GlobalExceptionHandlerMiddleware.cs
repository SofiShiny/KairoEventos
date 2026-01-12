using System.Net;
using System.Text.Json;
using MongoDB.Driver;

namespace Reportes.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path,
            Method = context.Request.Method
        };

        switch (exception)
        {
            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Error = "Solicitud inválida";
                response.Message = argEx.Message;
                response.StatusCode = 400;
                break;

            case KeyNotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Error = "Recurso no encontrado";
                response.Message = notFoundEx.Message;
                response.StatusCode = 404;
                break;

            case MongoException mongoEx:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Error = "Error de base de datos";
                response.Message = "El servicio de base de datos no está disponible temporalmente";
                response.StatusCode = 503;
                break;

            case TimeoutException timeoutEx:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Error = "Tiempo de espera agotado";
                response.Message = "La operación tardó demasiado tiempo en completarse";
                response.StatusCode = 408;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Error = "Error interno del servidor";
                response.Message = "Ocurrió un error inesperado. Por favor, intente nuevamente más tarde.";
                response.StatusCode = 500;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
