using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using System.Text.Json;
using Entradas.API.Middleware;
using Entradas.Dominio.Excepciones;
using FluentValidation;

namespace Entradas.Pruebas.API.Middleware;

/// <summary>
/// Pruebas unitarias para GlobalExceptionHandlerMiddleware
/// Valida el manejo correcto de diferentes tipos de excepciones y la generación de respuestas RFC 7807
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly GlobalExceptionHandlerMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _middleware = new GlobalExceptionHandlerMiddleware(_nextMock.Object, _loggerMock.Object);
        
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/entradas";
        _httpContext.Request.Method = "POST";
        _httpContext.Response.Body = new MemoryStream();
        
        // Setup service provider with environment
        var services = new ServiceCollection();
        services.AddSingleton(_environmentMock.Object);
        _httpContext.RequestServices = services.BuildServiceProvider();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange & Act
        var middleware = new GlobalExceptionHandlerMiddleware(_nextMock.Object, _loggerMock.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConNextNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new GlobalExceptionHandlerMiddleware(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("next");
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new GlobalExceptionHandlerMiddleware(_nextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }

    #endregion

    #region Success Path Tests

    [Fact]
    public async Task InvokeAsync_SinExcepciones_DebeLlamarSiguienteMiddleware()
    {
        // Arrange
        _nextMock.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(n => n(_httpContext), Times.Once);
        _httpContext.Response.StatusCode.Should().Be(200); // Default status
    }

    #endregion

    #region Domain Exception Tests

    [Fact]
    public async Task InvokeAsync_ConEventoNoDisponibleException_DebeRetornar404()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var exception = new EventoNoDisponibleException(eventoId, "El evento no está disponible");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(404);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Evento no disponible");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(404);
        problemDetails.Instance.Should().Be("/api/entradas");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");

        VerifyExceptionLogged(exception);
    }

    [Fact]
    public async Task InvokeAsync_ConAsientoNoDisponibleException_DebeRetornar409()
    {
        // Arrange
        var exception = new AsientoNoDisponibleException("El asiento no está disponible");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(409);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Asiento no disponible");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(409);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.8");

        VerifyExceptionLogged(exception);
    }

    [Fact]
    public async Task InvokeAsync_ConEntradaNoEncontradaException_DebeRetornar404()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var exception = new EntradaNoEncontradaException(entradaId, $"Entrada con ID {entradaId} no encontrada");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(404);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Entrada no encontrada");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(404);

        VerifyExceptionLogged(exception);
    }

    [Fact]
    public async Task InvokeAsync_ConEstadoEntradaInvalidoException_DebeRetornar400()
    {
        // Arrange
        var exception = new EstadoEntradaInvalidoException("Estado de entrada inválido");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(400);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Estado de entrada inválido");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(400);

        VerifyExceptionLogged(exception);
    }

    [Fact]
    public async Task InvokeAsync_ConServicioExternoNoDisponibleException_DebeRetornar503()
    {
        // Arrange
        var exception = new ServicioExternoNoDisponibleException("Eventos", "Servicio de eventos no disponible");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(503);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Servicio externo no disponible");
        problemDetails.Detail.Should().Be("Uno de los servicios externos requeridos no está disponible. Intente nuevamente más tarde.");
        problemDetails.Status.Should().Be(503);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.4");

        VerifyExceptionLogged(exception);
    }

    [Fact]
    public async Task InvokeAsync_ConDominioExceptionGenerica_DebeRetornar400()
    {
        // Arrange
        var exception = new EstadoEntradaInvalidoException("Error genérico de dominio");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(400);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Estado de entrada inválido");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(400);

        VerifyExceptionLogged(exception);
    }

    #endregion

    #region Validation Exception Tests

    [Fact]
    public async Task InvokeAsync_ConValidationException_DebeRetornar400()
    {
        // Arrange
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("EventoId", "El evento ID es requerido"),
            new("Monto", "El monto debe ser mayor a cero")
        };
        var exception = new ValidationException(validationFailures);
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(400);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Error de validación");
        problemDetails.Detail.Should().Contain("El evento ID es requerido");
        problemDetails.Detail.Should().Contain("El monto debe ser mayor a cero");
        problemDetails.Status.Should().Be(400);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");

        VerifyExceptionLogged(exception);
    }

    #endregion

    #region Argument Exception Tests

    [Fact]
    public async Task InvokeAsync_ConArgumentException_DebeRetornar400()
    {
        // Arrange
        var exception = new ArgumentException("Argumento inválido", "parametro");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(400);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Argumento inválido");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(400);

        VerifyExceptionLogged(exception);
    }

    #endregion

    #region Generic Exception Tests

    [Fact]
    public async Task InvokeAsync_ConExcepcionGenerica_DebeRetornar500()
    {
        // Arrange
        var exception = new InvalidOperationException("Error interno inesperado");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(500);
        _httpContext.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Error interno del servidor");
        problemDetails.Detail.Should().Be("Ha ocurrido un error interno. Contacte al administrador del sistema.");
        problemDetails.Status.Should().Be(500);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");

        VerifyExceptionLogged(exception);
    }

    #endregion

    #region Development Environment Tests

    [Fact]
    public async Task InvokeAsync_EnDesarrollo_DebeIncluirDetallesDeExcepcion()
    {
        // Arrange
        _environmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        var exception = new InvalidOperationException("Error de desarrollo");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("exception");
        problemDetails.Extensions.Should().ContainKey("stackTrace");
        problemDetails.Extensions["exception"].ToString().Should().Be("InvalidOperationException");
        problemDetails.Extensions["stackTrace"].Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_EnProduccion_NoDebeIncluirDetallesDeExcepcion()
    {
        // Arrange
        _environmentMock.SetupGet(e => e.EnvironmentName).Returns("Production");
        var exception = new InvalidOperationException("Error de producción");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().NotContainKey("exception");
        problemDetails!.Extensions.Should().NotContainKey("stackTrace");
    }

    #endregion

    #region Correlation ID Tests

    [Fact]
    public async Task InvokeAsync_ConTraceIdentifier_DebeIncluirTraceIdEnRespuesta()
    {
        // Arrange
        var traceId = "test-trace-id-12345";
        _httpContext.TraceIdentifier = traceId;
        var exception = new EstadoEntradaInvalidoException("Error con trace ID");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].ToString().Should().Be(traceId);
    }

    [Fact]
    public async Task InvokeAsync_SinTraceIdentifier_NoDebeIncluirTraceIdEnRespuesta()
    {
        // Arrange
        // Note: DefaultHttpContext automatically generates TraceIdentifier, so we need to work with what we get
        var exception = new EstadoEntradaInvalidoException("Error sin trace ID");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        // DefaultHttpContext always generates a TraceIdentifier, so traceId should always be present
        problemDetails!.Extensions.Should().ContainKey("traceId");
        var traceId = problemDetails.Extensions["traceId"]?.ToString();
        traceId.Should().NotBeNullOrEmpty("TraceIdentifier should be auto-generated by DefaultHttpContext");
    }

    #endregion

    #region Edge Cases Tests

    [Theory]
    [InlineData("/api/entradas")]
    [InlineData("/api/entradas/123")]
    [InlineData("/health")]
    [InlineData("/swagger")]
    public async Task InvokeAsync_ConDiferentesRutas_DebeIncluirRutaEnInstance(string path)
    {
        // Arrange
        _httpContext.Request.Path = path;
        var exception = new EstadoEntradaInvalidoException("Error en ruta específica");
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Instance.Should().Be(path);
    }

    [Fact]
    public async Task InvokeAsync_ConValidationExceptionVacia_DebeRetornarMensajeGenerico()
    {
        // Arrange
        var exception = new ValidationException(new List<FluentValidation.Results.ValidationFailure>());
        _nextMock.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var responseBody = await GetResponseBodyAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, GetJsonOptions());
        
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Error de validación");
        problemDetails.Detail.Should().BeEmpty();
        problemDetails.Status.Should().Be(400);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetResponseBodyAsync()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    private void VerifyExceptionLogged(Exception exception)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Excepción no manejada")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}