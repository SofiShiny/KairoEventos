using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Reportes.API.Middleware;
using Xunit;

namespace Reportes.Pruebas.API.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly DefaultHttpContext _httpContext;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddlewareTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_NoCorrelationIdInRequest_GeneratesNewCorrelationId()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        nextCalled.Should().BeTrue();
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        
        var correlationId = _httpContext.Response.Headers[CorrelationIdHeader].ToString();
        correlationId.Should().NotBeNullOrEmpty();
        
        // Verify it's a valid GUID
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdInRequest_UsesExistingCorrelationId()
    {
        // Arrange
        var existingCorrelationId = "test-correlation-id-12345";
        _httpContext.Request.Headers[CorrelationIdHeader] = existingCorrelationId;

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        _httpContext.Response.Headers[CorrelationIdHeader].ToString().Should().Be(existingCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToResponseHeaders()
    {
        // Arrange
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        _httpContext.Response.Headers[CorrelationIdHeader].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MultipleRequests_GeneratesDifferentCorrelationIds()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware((ctx) => Task.CompletedTask);
        
        var context1 = new DefaultHttpContext();
        context1.Response.Body = new MemoryStream();
        
        var context2 = new DefaultHttpContext();
        context2.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        var correlationId1 = context1.Response.Headers[CorrelationIdHeader].ToString();
        var correlationId2 = context2.Response.Headers[CorrelationIdHeader].ToString();

        correlationId1.Should().NotBeNullOrEmpty();
        correlationId2.Should().NotBeNullOrEmpty();
        correlationId1.Should().NotBe(correlationId2);
    }

    [Theory]
    [InlineData("correlation-123")]
    [InlineData("abc-def-ghi")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("custom-id-with-special-chars-!@#")]
    public async Task InvokeAsync_DifferentCorrelationIdFormats_PreservesExactValue(string correlationId)
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdHeader] = correlationId;
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeader].ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_EmptyCorrelationIdInRequest_UsesEmptyValue()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdHeader] = string.Empty;
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        // The middleware uses FirstOrDefault() which returns empty string if present
        var correlationId = _httpContext.Response.Headers[CorrelationIdHeader].ToString();
        correlationId.Should().Be(string.Empty);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionInNextDelegate_CorrelationIdStillAdded()
    {
        // Arrange
        RequestDelegate next = (ctx) => throw new Exception("Test exception");
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        var act = async () => await middleware.InvokeAsync(_httpContext);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        _httpContext.Response.Headers[CorrelationIdHeader].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdPropagatedThroughPipeline()
    {
        // Arrange
        var expectedCorrelationId = "pipeline-test-id";
        _httpContext.Request.Headers[CorrelationIdHeader] = expectedCorrelationId;
        
        string? capturedCorrelationId = null;
        RequestDelegate next = (ctx) =>
        {
            capturedCorrelationId = ctx.Response.Headers[CorrelationIdHeader].ToString();
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        capturedCorrelationId.Should().Be(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_GeneratedCorrelationIdIsValidGuid()
    {
        // Arrange
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var correlationId = _httpContext.Response.Headers[CorrelationIdHeader].ToString();
        var isValidGuid = Guid.TryParse(correlationId, out var parsedGuid);
        
        isValidGuid.Should().BeTrue();
        parsedGuid.Should().NotBe(Guid.Empty);
    }
}
