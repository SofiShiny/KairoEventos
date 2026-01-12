using FluentAssertions;
using Gateway.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gateway.Tests.Middleware;

/// <summary>
/// Tests unitarios para RequestLoggingMiddleware
/// Validates: Requirements 8.1, 8.5
/// </summary>
public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;

    public RequestLoggingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNextIsNull()
    {
        // Arrange & Act
        var act = () => new RequestLoggingMiddleware(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("next");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new RequestLoggingMiddleware(_nextMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task InvokeAsync_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await middleware.InvokeAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestStart()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestCompletion()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestMethod()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("POST")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestPath()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/usuarios/123");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("/api/usuarios/123")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogStatusCode()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        context.Response.StatusCode = 200;
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("200")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogDuration()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddRequestIdToContext()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("RequestId");
        context.Items["RequestId"].Should().NotBeNull();
        context.Items["RequestId"].Should().BeOfType<string>();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new InvalidOperationException("Test exception");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogDurationInError_WhenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new InvalidOperationException("Test exception");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRethrowException_WhenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var expectedException = new InvalidOperationException("Test exception");
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(expectedException);
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogTwoInformationMessages_OnSuccess()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestId_InAllLogMessages()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/eventos");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var requestId = context.Items["RequestId"] as string;
        requestId.Should().NotBeNullOrEmpty();

        // Verificar que el RequestId aparece en los logs
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(requestId!)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateUniqueRequestIds()
    {
        // Arrange
        var context1 = CreateHttpContext("GET", "/api/eventos");
        var context2 = CreateHttpContext("GET", "/api/usuarios");
        var middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        var requestId1 = context1.Items["RequestId"] as string;
        var requestId2 = context2.Items["RequestId"] as string;

        requestId1.Should().NotBeNullOrEmpty();
        requestId2.Should().NotBeNullOrEmpty();
        requestId1.Should().NotBe(requestId2);
    }

    [Fact]
    public void UseRequestLogging_ShouldRegisterMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseRequestLogging();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(appBuilder);
    }

    private static DefaultHttpContext CreateHttpContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.StatusCode = 200;
        return context;
    }
}
