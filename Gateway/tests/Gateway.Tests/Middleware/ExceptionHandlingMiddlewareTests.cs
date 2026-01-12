using FluentAssertions;
using Gateway.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Text.Json;

namespace Gateway.Tests.Middleware;

/// <summary>
/// Tests unitarios para ExceptionHandlingMiddleware
/// Validates: Requirements 2.2, 2.3, 2.4, 1.6
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNextIsNull()
    {
        // Arrange & Act
        var act = () => new ExceptionHandlingMiddleware(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("next");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new ExceptionHandlingMiddleware(_nextMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task InvokeAsync_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await middleware.InvokeAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate_WhenNoExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotModifyResponse_WhenNoExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    #region SecurityTokenExpiredException Tests

    [Fact]
    public async Task InvokeAsync_ShouldReturn401_WhenSecurityTokenExpiredExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new SecurityTokenExpiredException("Token expired");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnJsonResponse_WhenSecurityTokenExpiredExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new SecurityTokenExpiredException("Token expired");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().StartWith("application/json");
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Unauthorized");
        responseBody.Should().Contain("Token has expired");
        responseBody.Should().Contain("/api/eventos");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarning_WhenSecurityTokenExpiredExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new SecurityTokenExpiredException("Token expired");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token has expired")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SecurityTokenInvalidSignatureException Tests

    [Fact]
    public async Task InvokeAsync_ShouldReturn401_WhenSecurityTokenInvalidSignatureExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/usuarios");
        var expectedException = new SecurityTokenInvalidSignatureException("Invalid signature");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnJsonResponse_WhenSecurityTokenInvalidSignatureExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/usuarios");
        var expectedException = new SecurityTokenInvalidSignatureException("Invalid signature");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().StartWith("application/json");
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Unauthorized");
        responseBody.Should().Contain("Invalid token signature");
        responseBody.Should().Contain("/api/usuarios");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarning_WhenSecurityTokenInvalidSignatureExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/usuarios");
        var expectedException = new SecurityTokenInvalidSignatureException("Invalid signature");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid token signature")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SecurityTokenException Tests

    [Fact]
    public async Task InvokeAsync_ShouldReturn401_WhenSecurityTokenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/asientos");
        var expectedException = new SecurityTokenException("Invalid token");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnJsonResponse_WhenSecurityTokenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/asientos");
        var expectedException = new SecurityTokenException("Invalid token");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().StartWith("application/json");
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Unauthorized");
        responseBody.Should().Contain("Invalid token");
        responseBody.Should().Contain("/api/asientos");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarning_WhenSecurityTokenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/asientos");
        var expectedException = new SecurityTokenException("Invalid token");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid token")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region HttpRequestException Tests

    [Fact]
    public async Task InvokeAsync_ShouldReturn503_WhenHttpRequestExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/reportes");
        var expectedException = new HttpRequestException("Service unavailable");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnJsonResponse_WhenHttpRequestExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/reportes");
        var expectedException = new HttpRequestException("Service unavailable");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().StartWith("application/json");
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Service Unavailable");
        responseBody.Should().Contain("temporarily unavailable");
        responseBody.Should().Contain("/api/reportes");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogError_WhenHttpRequestExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/reportes");
        var expectedException = new HttpRequestException("Service unavailable");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Service unavailable")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Generic Exception Tests

    [Fact]
    public async Task InvokeAsync_ShouldReturn500_WhenGenericExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/entradas");
        var expectedException = new InvalidOperationException("Unexpected error");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnJsonResponse_WhenGenericExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/entradas");
        var expectedException = new InvalidOperationException("Unexpected error");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().StartWith("application/json");
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Internal Server Error");
        responseBody.Should().Contain("unexpected error occurred");
        responseBody.Should().Contain("/api/entradas");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogError_WhenGenericExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/entradas");
        var expectedException = new InvalidOperationException("Unexpected error");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task InvokeAsync_ShouldIncludeTimestamp_InErrorResponse()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new SecurityTokenExpiredException("Token expired");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("timestamp");
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludePath_InErrorResponse()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/usuarios/123");
        var expectedException = new HttpRequestException("Service down");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("/api/usuarios/123");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnValidJson_ForAllExceptionTypes()
    {
        // Arrange
        var exceptions = new Exception[]
        {
            new SecurityTokenExpiredException("Expired"),
            new SecurityTokenInvalidSignatureException("Invalid sig"),
            new SecurityTokenException("Invalid"),
            new HttpRequestException("Unavailable"),
            new InvalidOperationException("Generic")
        };

        foreach (var exception in exceptions)
        {
            var context = CreateHttpContext("GET", "/api/test");
            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);
            var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var responseBody = await GetResponseBodyAsync(context);
            var act = () => JsonDocument.Parse(responseBody);
            act.Should().NotThrow($"Response should be valid JSON for {exception.GetType().Name}");
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task InvokeAsync_ShouldNotWriteResponse_WhenResponseHasStarted()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        
        // Simular que la respuesta ya ha comenzado
        await context.Response.Body.WriteAsync(new byte[] { 1 });
        
        var expectedException = new SecurityTokenExpiredException("Token expired");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // No debe lanzar excepción al intentar escribir en una respuesta ya iniciada
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void UseExceptionHandling_ShouldRegisterMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseExceptionHandling();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(appBuilder);
    }

    #endregion

    private static DefaultHttpContext CreateHttpContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.StatusCode = 200;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> GetResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
