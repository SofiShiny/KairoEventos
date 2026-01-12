using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using Entradas.Dominio.Excepciones;
using Entradas.Infraestructura.ServiciosExternos;

namespace Entradas.Pruebas.Infraestructura.ServiciosExternos;

/// <summary>
/// Pruebas específicas para políticas de resiliencia de VerificadorAsientosHttp
/// Risk Hotspot Mitigation - Crap Score Original: 72 - Objetivo: <15
/// Enfoque simplificado para evitar complejidad de Polly en pruebas unitarias
/// </summary>
public class VerificadorAsientosHttpResilienceTests : IDisposable
{
    private readonly Mock<ILogger<VerificadorAsientosHttp>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly VerificadorAsientosOptions _options;
    private readonly VerificadorAsientosHttp _verificador;

    public VerificadorAsientosHttpResilienceTests()
    {
        _mockLogger = new Mock<ILogger<VerificadorAsientosHttp>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5002"),
            Timeout = TimeSpan.FromSeconds(2) // Timeout corto para pruebas rápidas
        };
        
        _options = new VerificadorAsientosOptions
        {
            BaseUrl = "http://localhost:5002",
            TimeoutSeconds = 1,
            MaxRetries = 1,
            CircuitBreakerFailureThreshold = 2,
            CircuitBreakerDurationSeconds = 1
        };

        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        _verificador = new VerificadorAsientosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);
    }

    #region Basic Resilience Tests

    [Fact]
    public async Task AsientoDisponibleAsync_ConHttpRequestException_DebeRetornarExcepcionControlada()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.AsientoDisponibleAsync(eventoId, asientoId))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error de comunicación con el servicio de asientos*");
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConTaskCanceledException_DebeRetornarExcepcionControlada()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.AsientoDisponibleAsync(eventoId, asientoId))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado al verificar el asiento*");
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConExcepcionGenerica_DebeRetornarExcepcionControlada()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.AsientoDisponibleAsync(eventoId, asientoId))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado al verificar el asiento*");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConHttpRequestException_DebeRetornarExcepcionControlada()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error de comunicación con el servicio de asientos*");
    }

    [Fact]
    public async Task ObtenerInfoAsientoAsync_ConHttpRequestException_DebeRetornarExcepcionControlada()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.ObtenerInfoAsientoAsync(eventoId, asientoId))
            .Should().ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error de comunicación con el servicio de asientos*");
    }

    #endregion

    #region HTTP Status Code Tests

    [Fact]
    public async Task AsientoDisponibleAsync_ConStatus500_DebeReintentar()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var contadorIntentos = 0;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                contadorIntentos++;
                if (contadorIntentos == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeTrue();
        contadorIntentos.Should().BeGreaterThan(1); // Verificar que se reintentó
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConStatus404_NoDebeReintentar()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var contadorIntentos = 0;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                contadorIntentos++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeFalse();
        contadorIntentos.Should().Be(1); // No debe reintentar para 404
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConStatus409_DebeRetornarAsientoNoDisponible()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict));

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should().ThrowAsync<AsientoNoDisponibleException>()
            .WithMessage($"El asiento {asientoId} ya está reservado");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task AsientoDisponibleAsync_ConExito_DebeLoggearDebug()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verificando disponibilidad")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConError_DebeLoggearError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service failure"));

        // Act
        try
        {
            await _verificador.AsientoDisponibleAsync(eventoId, asientoId);
        }
        catch (ServicioExternoNoDisponibleException)
        {
            // Esperado
        }

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error HTTP al verificar asiento")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task AsientoDisponibleAsync_ConMultiplesLlamadas_DebeCompletarseRapidamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoIds = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList(); // Reducido para pruebas más rápidas

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tareas = asientoIds.Select(id => _verificador.AsientoDisponibleAsync(eventoId, id));
        var resultados = await Task.WhenAll(tareas);
        stopwatch.Stop();

        // Assert
        resultados.Should().HaveCount(3);
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10)); // Tiempo más realista
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task AsientoDisponibleAsync_ConAsientoIdNulo_DebeRetornarTrue()
    {
        // Arrange
        var eventoId = Guid.NewGuid();

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, null);

        // Assert
        resultado.Should().BeTrue();
        
        // Verificar que no se hizo llamada HTTP
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConEventoIdVacio_DebeRetornarArgumentException()
    {
        // Arrange
        var asientoId = Guid.NewGuid();

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.AsientoDisponibleAsync(Guid.Empty, asientoId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*El ID del evento no puede ser vacío*");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConDuracionNegativa_DebeRetornarArgumentException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(-5);

        // Act & Assert
        await FluentActions.Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*La duración debe ser mayor a cero*");
    }

    [Fact]
    public async Task ObtenerInfoAsientoAsync_ConAsientoNoEncontrado_DebeRetornarNull()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var resultado = await _verificador.ObtenerInfoAsientoAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerInfoAsientoAsync_ConRespuestaExitosa_DebeRetornarAsientoInfo()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var asientoDto = new
        {
            Id = asientoId,
            Seccion = "VIP",
            Fila = 1,
            Numero = 1,
            EstaDisponible = true,
            PrecioAdicional = 50.0m
        };

        var jsonContent = System.Text.Json.JsonSerializer.Serialize(asientoDto);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _verificador.ObtenerInfoAsientoAsync(eventoId, asientoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(asientoId);
        resultado.Seccion.Should().Be("VIP");
        resultado.Fila.Should().Be(1);
        resultado.Numero.Should().Be(1);
        resultado.EstaDisponible.Should().BeTrue();
        resultado.PrecioAdicional.Should().Be(50.0m);
    }

    #endregion

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}