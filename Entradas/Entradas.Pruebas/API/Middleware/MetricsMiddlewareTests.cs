using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using FluentAssertions;
using System.Diagnostics;
using System.Linq;
using System.Collections.Concurrent;
using Entradas.API.Middleware;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.API.Middleware;

/// <summary>
/// Pruebas unitarias para MetricsMiddleware
/// Valida la recopilación correcta de métricas de performance y distributed tracing
/// </summary>
public class MetricsMiddlewareTests
{
    private readonly Mock<ILogger<MetricsMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<IEntradasMetrics> _metricsMock;
    private readonly ActivitySource _activitySource;
    private readonly MetricsMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public MetricsMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<MetricsMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _metricsMock = new Mock<IEntradasMetrics>();
        _activitySource = new ActivitySource("Entradas.API.Tests");
        _middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, _activitySource);
        
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/entradas";
        _httpContext.Request.Method = "POST";
        _httpContext.Request.Scheme = "https";
        _httpContext.Request.Host = new HostString("localhost:5001");
        _httpContext.Response.StatusCode = 200;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange & Act
        var middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, _activitySource);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConNextNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new MetricsMiddleware(null!, _loggerMock.Object, _metricsMock.Object, _activitySource);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("next");
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new MetricsMiddleware(_nextMock.Object, null!, _metricsMock.Object, _activitySource);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ConMetricsNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, null!, _activitySource);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("metrics");
    }

    [Fact]
    public void Constructor_ConActivitySourceNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("activitySource");
    }

    #endregion

    #region Success Path Tests

    [Fact]
    public async Task InvokeAsync_ConRequestExitoso_DebeRegistrarMetricasDeExito()
    {
        // Arrange
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(n => n(_httpContext), Times.Once);
        
        // Verify debug logging for API endpoints
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request POST /api/entradas completado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConRequestExitoso_DebeCrearActivityConTagsCorrectos()
    {
        // Arrange
        var sourceName = $"Entradas.API.Tests.Success.{Guid.NewGuid()}";
        using var testSource = new ActivitySource(sourceName);
        var middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, testSource);

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => activities.Add(activity);

        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        
        activity.DisplayName.Should().Be("POST /api/entradas");
        activity.GetTagItem("http.method").Should().Be("POST");
        activity.GetTagItem("http.path").Should().Be("/api/entradas");
        activity.GetTagItem("http.scheme").Should().Be("https");
        activity.GetTagItem("http.host").Should().Be("localhost:5001");
        activity.GetTagItem("http.status_code").Should().Be(200);
        activity.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task InvokeAsync_ConCorrelationId_DebeAgregarCorrelationIdAActivity()
    {
        // Arrange
        var sourceName = $"Entradas.API.Tests.Correlation.{Guid.NewGuid()}";
        using var testSource = new ActivitySource(sourceName);
        var middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, testSource);

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => activities.Add(activity);

        var correlationId = "test-correlation-id-12345";
        _httpContext.Request.Headers.Append("X-Correlation-ID", new StringValues(correlationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        activity.GetTagItem("correlation.id").Should().Be(correlationId);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task InvokeAsync_ConExcepcion_DebeRegistrarMetricasDeError()
    {
        // Arrange
        var exception = new InvalidOperationException("Error de prueba");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(_httpContext));

        // Verify error logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error no manejado en request POST /api/entradas")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConExcepcion_DebeMarcarActivityComoError()
    {
        // Arrange
        var sourceName = $"Entradas.API.Tests.Exception.{Guid.NewGuid()}";
        using var testSource = new ActivitySource(sourceName);
        var middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, testSource);

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => activities.Add(activity);

        var exception = new InvalidOperationException("Error de prueba");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(_httpContext));

        // Assert
        var relevantActivities = activities.ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Error de prueba");
        activity.GetTagItem("exception.type").Should().Be("InvalidOperationException");
        activity.GetTagItem("exception.message").Should().Be("Error de prueba");
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task InvokeAsync_ConHealthCheckExitoso_DebeRegistrarMetricasDeHealth()
    {
        // Arrange
        _httpContext.Request.Path = "/health";
        _httpContext.Response.StatusCode = 200;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _metricsMock.Verify(
            m => m.RecordHealthCheckResult(
                "general",
                "healthy",
                It.IsAny<double>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConHealthCheckEspecifico_DebeUsarNombreCorrectoDeLCheck()
    {
        // Arrange
        _httpContext.Request.Path = "/health/database";
        _httpContext.Response.StatusCode = 200;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _metricsMock.Verify(
            m => m.RecordHealthCheckResult(
                "database",
                "healthy",
                It.IsAny<double>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConHealthCheckFallido_DebeRegistrarComoUnhealthy()
    {
        // Arrange
        _httpContext.Request.Path = "/health/rabbitmq";
        _httpContext.Response.StatusCode = 503;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _metricsMock.Verify(
            m => m.RecordHealthCheckResult(
                "rabbitmq",
                "unhealthy",
                It.IsAny<double>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConHealthCheckDegradado_DebeRegistrarComoDegraded()
    {
        // Arrange
        _httpContext.Request.Path = "/health/external";
        _httpContext.Response.StatusCode = 429; // Rate limited or other non-200/503 status
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _metricsMock.Verify(
            m => m.RecordHealthCheckResult(
                "external",
                "degraded",
                It.IsAny<double>()),
            Times.Once);
    }

    #endregion

    #region HTTP Status Code Tests

    [Theory]
    [InlineData(200, ActivityStatusCode.Ok)]
    [InlineData(201, ActivityStatusCode.Ok)]
    [InlineData(204, ActivityStatusCode.Ok)]
    [InlineData(299, ActivityStatusCode.Ok)]
    public async Task InvokeAsync_ConStatusCodeExitoso_DebeMarcarActivityComoOk(int statusCode, ActivityStatusCode expectedStatus)
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Entradas.API.Tests",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => 
        {
            if (activity.Source.Name == "Entradas.API.Tests")
                activities.Add(activity);
        };

        _httpContext.Response.StatusCode = statusCode;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.Where(a => a.Source.Name == "Entradas.API.Tests").ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        
        activity.Status.Should().Be(expectedStatus);
        activity.GetTagItem("http.status_code").Should().Be(statusCode);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(503)]
    public async Task InvokeAsync_ConStatusCodeError_DebeMarcarActivityComoError(int statusCode)
    {
        // Arrange
        var sourceName = $"Entradas.API.Tests.ErrorStatus.{Guid.NewGuid()}";
        using var testSource = new ActivitySource(sourceName);
        var middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, testSource);

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => activities.Add(activity);

        _httpContext.Response.StatusCode = statusCode;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be($"HTTP {statusCode}");
        activity.GetTagItem("http.status_code").Should().Be(statusCode);
    }

    #endregion

    #region Different Paths Tests

    [Theory]
    [InlineData("/api/entradas", true)]
    [InlineData("/api/entradas/123", true)]
    [InlineData("/api/usuarios", true)]
    [InlineData("/health", false)]
    [InlineData("/swagger", false)]
    [InlineData("/metrics", false)]
    public async Task InvokeAsync_ConDiferentesRutas_DebeLoguearSoloEndpointsAPI(string path, bool shouldLogDebug)
    {
        // Arrange
        _httpContext.Request.Path = path;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        if (shouldLogDebug)
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Request POST {path} completado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        else
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Request POST {path} completado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task InvokeAsync_DebeRegistrarTiempoDeDuracion()
    {
        // Arrange
        var delay = TimeSpan.FromMilliseconds(100);
        _nextMock.Setup(n => n(_httpContext))
                 .Returns(async () => await Task.Delay(delay));

        // Act
        var stopwatch = Stopwatch.StartNew();
        await _middleware.InvokeAsync(_httpContext);
        stopwatch.Stop();

        // Assert
        // Verify that some time was measured (should be at least the delay time)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completado en") && 
                                              v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConRequestLento_DebeIncluirTiempoEnActivity()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Entradas.API.Tests",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => 
        {
            if (activity.Source.Name == "Entradas.API.Tests")
                activities.Add(activity);
        };

        var delay = TimeSpan.FromMilliseconds(50);
        _nextMock.Setup(n => n(_httpContext))
                 .Returns(async () => await Task.Delay(delay));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.Where(a => a.Source.Name == "Entradas.API.Tests").ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        
        var responseTime = activity.GetTagItem("http.response_time_ms");
        responseTime.Should().NotBeNull();
        
        // Response time should be at least the delay time
        var responseTimeValue = Convert.ToDouble(responseTime);
        responseTimeValue.Should().BeGreaterThanOrEqualTo(delay.TotalMilliseconds);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task InvokeAsync_ConPathVacio_DebeUsarPathVacio()
    {
        // Arrange
        _httpContext.Request.Path = "";
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        // Should not throw and should handle empty path gracefully
        _nextMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConPathNulo_DebeUsarStringVacio()
    {
        // Arrange
        _httpContext.Request.Path = new PathString(); // Default/null path
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        // Should not throw and should handle null path gracefully
        _nextMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SinCorrelationIdHeader_NoDebeAgregarCorrelationIdAActivity()
    {
        // Arrange
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Entradas.API.Tests",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => 
        {
            if (activity.Source.Name == "Entradas.API.Tests")
                activities.Add(activity);
        };

        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.Where(a => a.Source.Name == "Entradas.API.Tests").ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        activity.GetTagItem("correlation.id").Should().BeNull();
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_ConDiferentesMetodosHTTP_DebeRegistrarMetodoCorrectamente(string method)
    {
        // Arrange
        var sourceName = $"Entradas.API.Tests.Methods.{method}.{Guid.NewGuid()}";
        using var testSource = new ActivitySource(sourceName);
        var middleware = new MetricsMiddleware(_nextMock.Object, _loggerMock.Object, _metricsMock.Object, testSource);

        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        var activities = new ConcurrentBag<Activity>();
        listener.ActivityStarted = activity => activities.Add(activity);

        _httpContext.Request.Method = method;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        var relevantActivities = activities.ToList();
        relevantActivities.Should().HaveCount(1);
        var activity = relevantActivities[0];
        
        activity.DisplayName.Should().Be($"{method} /api/entradas");
        activity.GetTagItem("http.method").Should().Be(method);
    }

    #endregion
    [Fact]
    public void UseMetrics_DebeRegistrarMiddleware()
    {
        // Arrange
        var builder = new Mock<IApplicationBuilder>();
        builder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
               .Returns(builder.Object);
        
        // Act & Assert
        builder.Object.UseMetrics().Should().NotBeNull();
    }
}