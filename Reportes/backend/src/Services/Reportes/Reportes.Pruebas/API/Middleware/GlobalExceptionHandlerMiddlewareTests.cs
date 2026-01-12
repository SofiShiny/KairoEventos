using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Reportes.API.Middleware;
using Xunit;

namespace Reportes.Pruebas.API.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        _httpContext.Request.Path = "/api/reportes/test";
        _httpContext.Request.Method = "GET";
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        nextCalled.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be(200); // Default status code
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400BadRequest()
    {
        // Arrange
        var exceptionMessage = "Parámetro inválido";
        RequestDelegate next = (ctx) => throw new ArgumentException(exceptionMessage);
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(400);
        errorResponse.Error.Should().Be("Solicitud inválida");
        errorResponse.Message.Should().Be(exceptionMessage);
        errorResponse.Path.Should().Be("/api/reportes/test");
        errorResponse.Method.Should().Be("GET");
        errorResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Returns404NotFound()
    {
        // Arrange
        var exceptionMessage = "Recurso no encontrado";
        RequestDelegate next = (ctx) => throw new KeyNotFoundException(exceptionMessage);
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(404);
        errorResponse.Error.Should().Be("Recurso no encontrado");
        errorResponse.Message.Should().Be(exceptionMessage);
        errorResponse.Path.Should().Be("/api/reportes/test");
        errorResponse.Method.Should().Be("GET");
    }

    [Fact]
    public async Task InvokeAsync_MongoException_Returns503ServiceUnavailable()
    {
        // Arrange
        RequestDelegate next = (ctx) => throw new MongoException("Connection failed");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(503);
        errorResponse.Error.Should().Be("Error de base de datos");
        errorResponse.Message.Should().Be("El servicio de base de datos no está disponible temporalmente");
    }

    [Fact]
    public async Task InvokeAsync_TimeoutException_Returns408RequestTimeout()
    {
        // Arrange
        RequestDelegate next = (ctx) => throw new TimeoutException("Operation timed out");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.RequestTimeout);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(408);
        errorResponse.Error.Should().Be("Tiempo de espera agotado");
        errorResponse.Message.Should().Be("La operación tardó demasiado tiempo en completarse");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500InternalServerError()
    {
        // Arrange
        RequestDelegate next = (ctx) => throw new InvalidOperationException("Unexpected error");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(500);
        errorResponse.Error.Should().Be("Error interno del servidor");
        errorResponse.Message.Should().Be("Ocurrió un error inesperado. Por favor, intente nuevamente más tarde.");
    }

    [Fact]
    public async Task InvokeAsync_AnyException_ReturnsValidJsonFormat()
    {
        // Arrange
        RequestDelegate next = (ctx) => throw new Exception("Test exception");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.ContentType.Should().Be("application/json");
        
        var responseBody = await GetResponseBodyAsync();
        
        // Verify it's valid JSON by deserializing
        var deserializeAction = () => JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        deserializeAction.Should().NotThrow();
        
        var errorResponse = deserializeAction();
        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().BeGreaterThan(0);
        errorResponse.Error.Should().NotBeNullOrEmpty();
        errorResponse.Message.Should().NotBeNullOrEmpty();
        errorResponse.Path.Should().NotBeNullOrEmpty();
        errorResponse.Method.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_AnyException_LogsError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        RequestDelegate next = (ctx) => throw exception;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Excepción no controlada")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_MultipleExceptionTypes_ReturnsCorrectStatusCodes()
    {
        // Test ArgumentException
        await TestExceptionStatusCode<ArgumentException>(400);
        
        // Test KeyNotFoundException
        await TestExceptionStatusCode<KeyNotFoundException>(404);
        
        // Test TimeoutException
        await TestExceptionStatusCode<TimeoutException>(408);
        
        // Test generic Exception
        await TestExceptionStatusCode<Exception>(500);
    }

    [Theory]
    [InlineData("/api/reportes/ventas", "GET")]
    [InlineData("/api/reportes/asistencia/123", "POST")]
    [InlineData("/api/reportes/auditoria", "PUT")]
    [InlineData("/api/reportes/conciliacion", "DELETE")]
    public async Task InvokeAsync_DifferentPathsAndMethods_IncludesCorrectPathAndMethod(string path, string method)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _httpContext.Request.Method = method;
        
        RequestDelegate next = (ctx) => throw new Exception("Test");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Path.Should().Be(path);
        errorResponse.Method.Should().Be(method);
    }

    [Fact]
    public async Task InvokeAsync_JsonSerializationUsesCamelCase()
    {
        // Arrange
        RequestDelegate next = (ctx) => throw new Exception("Test");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        
        // Verify camelCase by checking the raw JSON string
        responseBody.Should().Contain("\"statusCode\":");
        responseBody.Should().Contain("\"error\":");
        responseBody.Should().Contain("\"message\":");
        responseBody.Should().Contain("\"timestamp\":");
        responseBody.Should().Contain("\"path\":");
        responseBody.Should().Contain("\"method\":");
        
        // Should NOT contain PascalCase
        responseBody.Should().NotContain("\"StatusCode\":");
        responseBody.Should().NotContain("\"Error\":");
        responseBody.Should().NotContain("\"Message\":");
    }

    private async Task<string> GetResponseBodyAsync()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private async Task TestExceptionStatusCode<TException>(int expectedStatusCode) where TException : Exception, new()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";
        context.Request.Method = "GET";
        
        RequestDelegate next = (ctx) => throw new TException();
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(expectedStatusCode);
    }
}
