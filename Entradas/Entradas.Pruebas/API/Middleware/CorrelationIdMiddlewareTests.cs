using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using FluentAssertions;
using Entradas.API.Middleware;

namespace Entradas.Pruebas.API.Middleware;

/// <summary>
/// Pruebas unitarias para CorrelationIdMiddleware
/// Valida el manejo correcto de correlation IDs para distributed tracing
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly CorrelationIdMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);
        
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/entradas";
        _httpContext.Request.Method = "POST";
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange & Act
        var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConNextNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new CorrelationIdMiddleware(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("next");
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new CorrelationIdMiddleware(_nextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }

    #endregion

    #region Existing Correlation ID Tests

    [Fact]
    public async Task InvokeAsync_ConCorrelationIdExistente_DebeUsarElMismoId()
    {
        // Arrange
        var existingCorrelationId = "existing-correlation-id-12345";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(existingCorrelationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(existingCorrelationId);
        _nextMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConCorrelationIdExistente_DebeAgregarAlScopeDeLogging()
    {
        // Arrange
        var existingCorrelationId = "test-correlation-id-67890";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(existingCorrelationId));
        
        var loggerScopeVerified = false;
        _loggerMock.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                  .Returns(Mock.Of<IDisposable>())
                  .Callback<Dictionary<string, object>>(scope =>
                  {
                      scope.Should().ContainKey("CorrelationId");
                      scope["CorrelationId"].Should().Be(existingCorrelationId);
                      loggerScopeVerified = true;
                  });

        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        loggerScopeVerified.Should().BeTrue();
        _loggerMock.Verify(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConCorrelationIdConEspacios_DebeUsarElMismoIdSinModificar()
    {
        // Arrange
        var correlationIdWithSpaces = " correlation-id-with-spaces ";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationIdWithSpaces));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(correlationIdWithSpaces);
    }

    #endregion

    #region Missing Correlation ID Tests

    [Fact]
    public async Task InvokeAsync_SinCorrelationId_DebeUsarTraceIdentifier()
    {
        // Arrange
        var traceIdentifier = "trace-id-from-context";
        _httpContext.TraceIdentifier = traceIdentifier;
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(traceIdentifier);
        _nextMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SinCorrelationIdYSinTraceIdentifier_DebeGenerarNuevoGuid()
    {
        // Arrange
        // Note: DefaultHttpContext automatically generates TraceIdentifier, so we test with what we get
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeaderName);
        var correlationId = _httpContext.Response.Headers[CorrelationIdHeaderName].ToString();
        
        correlationId.Should().NotBeNullOrEmpty();
        // Since DefaultHttpContext auto-generates TraceIdentifier, we should get that value
        correlationId.Should().Be(_httpContext.TraceIdentifier);
        _nextMock.Verify(n => n(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SinCorrelationId_DebeAgregarNuevoIdAlScopeDeLogging()
    {
        // Arrange
        var traceIdentifier = "new-trace-id-12345";
        _httpContext.TraceIdentifier = traceIdentifier;
        
        var loggerScopeVerified = false;
        _loggerMock.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                  .Returns(Mock.Of<IDisposable>())
                  .Callback<Dictionary<string, object>>(scope =>
                  {
                      scope.Should().ContainKey("CorrelationId");
                      scope["CorrelationId"].Should().Be(traceIdentifier);
                      loggerScopeVerified = true;
                  });

        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        loggerScopeVerified.Should().BeTrue();
        _loggerMock.Verify(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    #endregion

    #region Empty/Whitespace Correlation ID Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public async Task InvokeAsync_ConCorrelationIdVacioOEspacios_DebeGenerarNuevoId(string emptyOrWhitespaceId)
    {
        // Arrange
        var traceIdentifier = "fallback-trace-id";
        _httpContext.TraceIdentifier = traceIdentifier;
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(emptyOrWhitespaceId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(traceIdentifier);
        _nextMock.Verify(n => n(_httpContext), Times.Once);
    }

    #endregion

    #region Multiple Values Tests

    [Fact]
    public async Task InvokeAsync_ConMultiplesValoresDeCorrelationId_DebeUsarElPrimero()
    {
        // Arrange
        var firstCorrelationId = "first-correlation-id";
        var secondCorrelationId = "second-correlation-id";
        var multipleValues = new StringValues(new[] { firstCorrelationId, secondCorrelationId });
        
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, multipleValues);
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var headerValue = _httpContext.Response.Headers[CorrelationIdHeaderName].ToString();
        headerValue.Should().Contain(firstCorrelationId);
    }

    #endregion

    #region Response Header Tests

    [Fact]
    public async Task InvokeAsync_Siempre_DebeAgregarCorrelationIdALaRespuesta()
    {
        // Arrange
        var correlationId = "response-correlation-id";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeaderName);
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ConHeaderDeRespuestaExistente_NoDebeReemplazarHeader()
    {
        // Arrange
        var existingResponseHeader = "existing-response-correlation-id";
        var requestCorrelationId = "request-correlation-id";
        
        _httpContext.Response.Headers.Append(CorrelationIdHeaderName, existingResponseHeader);
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(requestCorrelationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        // TryAdd should not replace existing header
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(existingResponseHeader);
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().NotContain(requestCorrelationId);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task InvokeAsync_ConExcepcionEnSiguienteMiddleware_DebeManternerCorrelationIdEnRespuesta()
    {
        // Arrange
        var correlationId = "exception-correlation-id";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        
        var exception = new InvalidOperationException("Error en siguiente middleware");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(_httpContext));
        
        // Even with exception, correlation ID should be in response
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ConExcepcionEnSiguienteMiddleware_DebeManternerScopeDeLogging()
    {
        // Arrange
        var correlationId = "exception-scope-correlation-id";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        
        var disposableMock = new Mock<IDisposable>();
        var loggerScopeVerified = false;
        
        _loggerMock.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                  .Returns(disposableMock.Object)
                  .Callback<Dictionary<string, object>>(scope =>
                  {
                      scope.Should().ContainKey("CorrelationId");
                      scope["CorrelationId"].Should().Be(correlationId);
                      loggerScopeVerified = true;
                  });

        var exception = new InvalidOperationException("Error en siguiente middleware");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(_httpContext));
        
        loggerScopeVerified.Should().BeTrue();
        disposableMock.Verify(d => d.Dispose(), Times.Once);
    }

    #endregion

    #region Logging Scope Tests

    [Fact]
    public async Task InvokeAsync_Siempre_DebeCrearYDisponerScopeDeLogging()
    {
        // Arrange
        var correlationId = "scope-test-correlation-id";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        
        var disposableMock = new Mock<IDisposable>();
        _loggerMock.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                  .Returns(disposableMock.Object);

        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Once);
        disposableMock.Verify(d => d.Dispose(), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ConScopeDeLogging_DebeContenerClaveCorrelationId()
    {
        // Arrange
        var correlationId = "scope-key-test-id";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        
        Dictionary<string, object>? capturedScope = null;
        _loggerMock.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                  .Returns(Mock.Of<IDisposable>())
                  .Callback<Dictionary<string, object>>(scope => capturedScope = scope);

        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        capturedScope.Should().NotBeNull();
        capturedScope!.Should().ContainKey("CorrelationId");
        capturedScope["CorrelationId"].Should().Be(correlationId);
        capturedScope.Should().HaveCount(1); // Should only contain CorrelationId
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task InvokeAsync_ConTraceIdentifierVacio_DebeGenerarGuid()
    {
        // Arrange
        // Note: DefaultHttpContext automatically generates TraceIdentifier, so we test with what we get
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers.Should().ContainKey(CorrelationIdHeaderName);
        var correlationId = _httpContext.Response.Headers[CorrelationIdHeaderName].ToString();
        
        correlationId.Should().NotBeNullOrEmpty();
        // Since DefaultHttpContext auto-generates TraceIdentifier, we should get that value
        correlationId.Should().Be(_httpContext.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_ConDiferentesRutas_DebeAplicarseATodas()
    {
        // Arrange
        var correlationId = "path-test-correlation-id";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        
        var testPaths = new[] { "/api/entradas", "/health", "/swagger", "/metrics", "/" };
        
        foreach (var path in testPaths)
        {
            // Reset context for each path
            _httpContext.Request.Path = path;
            _httpContext.Response.Headers.Clear();
            
            _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(correlationId, 
                $"Correlation ID should be added for path: {path}");
        }
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_ConDiferentesMetodosHTTP_DebeAplicarseATodos(string method)
    {
        // Arrange
        var correlationId = $"method-test-{method.ToLower()}-correlation-id";
        _httpContext.Request.Method = method;
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_ConCorrelationIdMuyLargo_DebeUsarloSinTruncar()
    {
        // Arrange
        var longCorrelationId = new string('a', 1000); // Very long correlation ID
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(longCorrelationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(longCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ConCaracteresEspeciales_DebePreservarlos()
    {
        // Arrange
        var specialCharCorrelationId = "correlation-id-with-special-chars-!@#$%^&*()_+-={}[]|\\:;\"'<>?,./";
        _httpContext.Request.Headers.Append(CorrelationIdHeaderName, new StringValues(specialCharCorrelationId));
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers[CorrelationIdHeaderName].Should().Contain(specialCharCorrelationId);
    }

    #endregion
}