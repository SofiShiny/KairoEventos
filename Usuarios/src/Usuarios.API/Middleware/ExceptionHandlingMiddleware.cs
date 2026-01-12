using System.Net;
using System.Text.Json;
using FluentValidation;
using Usuarios.Dominio.Excepciones;

namespace Usuarios.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (UsuarioNoEncontradoException ex)
        {
            _logger.LogWarning(ex, "Usuario no encontrado: {UsuarioId}", ex.UsuarioId);
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound, ex.Message);
        }
        catch (CorreoDuplicadoException ex)
        {
            _logger.LogWarning(ex, "Correo duplicado: {Correo}", ex.Correo);
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UsernameDuplicadoException ex)
        {
            _logger.LogWarning(ex, "Username duplicado: {Username}", ex.Username);
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación");
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            var detail = ex.Data["ResponseBody"]?.ToString() ?? ex.Message;
            _logger.LogError(ex, "Error al comunicarse con Keycloak: {Detail}", detail);
            await HandleExceptionAsync(
                context,
                ex,
                HttpStatusCode.BadGateway,
                $"Error al comunicarse con el servicio de autenticación: {detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
            await HandleExceptionAsync(
                context,
                ex,
                HttpStatusCode.InternalServerError,
                "Ocurrió un error interno en el servidor");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message = message,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            status = (int)HttpStatusCode.BadRequest,
            message = "Errores de validación",
            errors = errors,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
