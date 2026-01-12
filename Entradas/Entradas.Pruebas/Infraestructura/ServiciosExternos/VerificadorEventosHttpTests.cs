using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using Entradas.Dominio.Excepciones;
using Entradas.Infraestructura.ServiciosExternos;
using System.Text.Json;

namespace Entradas.Pruebas.Infraestructura.ServiciosExternos;

/// <summary>
/// Pruebas comprehensivas para VerificadorEventosHttp
/// Incluye políticas de resiliencia, circuit breaker, retry y timeout
/// </summary>
public class VerificadorEventosHttpTests
{
    private readonly Mock<ILogger<VerificadorEventosHttp>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly VerificadorEventosOptions _options;
    private readonly VerificadorEventosHttp _verificador;

    public VerificadorEventosHttpTests()
    {
        _mockLogger = new Mock<ILogger<VerificadorEventosHttp>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };
        
        _options = new VerificadorEventosOptions
        {
            BaseUrl = "http://localhost:5001",
            TimeoutSeconds = 1,
            MaxRetries = 0, // Desactivar por defecto para velocidad
            CircuitBreakerFailureThreshold = 2,
            CircuitBreakerDurationSeconds = 1
        };

        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        _verificador = new VerificadorEventosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConEventoDisponible_DeberiaRetornarTrue()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConEventoNoDisponible_DeberiaRetornarFalse()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConEventoIdVacio_DeberiaLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.EventoExisteYDisponibleAsync(Guid.Empty))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("El ID del evento no puede ser vacío*");
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConErrorHttp_DeberiaLanzarServicioExternoNoDisponibleException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error de conexión"));

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.EventoExisteYDisponibleAsync(eventoId))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error de comunicación con el servicio de eventos*");
    }

    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaCrearInstancia()
    {
        // Arrange & Act
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        var verificador = new VerificadorEventosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);

        // Assert
        verificador.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConHttpClientNulo_DeberiaLanzarArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        // Act & Assert
        FluentActions
            .Invoking(() => new VerificadorEventosHttp(null!, _mockLogger.Object, mockOptions.Object))
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DeberiaLanzarArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        // Act & Assert
        FluentActions
            .Invoking(() => new VerificadorEventosHttp(_httpClient, null!, mockOptions.Object))
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ConOptionsNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        FluentActions
            .Invoking(() => new VerificadorEventosHttp(_httpClient, _mockLogger.Object, null!))
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task EventoExisteYDisponibleAsync_ConErroresHttp5xx_DeberiaRetornarFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(statusCode);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Gone)]
    public async Task EventoExisteYDisponibleAsync_ConErroresHttp4xx_DeberiaRetornarFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(statusCode);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConTimeout_DeberiaLanzarServicioExternoNoDisponibleException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.EventoExisteYDisponibleAsync(eventoId))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado*");
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConCancellationToken_DebeRespetarCancelacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.EventoExisteYDisponibleAsync(eventoId, cts.Token))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado*");
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConRespuestaJson_DeberiaProcessarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var eventoInfo = new { Id = eventoId, Nombre = "Concierto Test", Disponible = true };
        var jsonContent = JsonSerializer.Serialize(eventoInfo);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConRespuestaVacia_DeberiaRetornarTrue()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConMultiplesLlamadasSimultaneas_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var eventoIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var tasks = eventoIds.Select(id => _verificador.EventoExisteYDisponibleAsync(id));
        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().AllSatisfy(resultado => resultado.Should().BeTrue());
        resultados.Should().HaveCount(10);
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConErrorDeRed_DeberiaLanzarServicioExternoNoDisponibleException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("No se pudo resolver el nombre del host"));

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.EventoExisteYDisponibleAsync(eventoId))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error de comunicación con el servicio de eventos*");
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/xml")]
    [InlineData("text/plain")]
    public async Task EventoExisteYDisponibleAsync_ConDiferentesContentTypes_DeberiaFuncionarCorrectamente(string contentType)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, contentType)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_DeberiaEnviarHeadersCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        HttpRequestMessage? requestCapturado = null;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => requestCapturado = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        requestCapturado.Should().NotBeNull();
        requestCapturado!.Method.Should().Be(HttpMethod.Get);
        requestCapturado.RequestUri.Should().NotBeNull();
        requestCapturado.RequestUri!.ToString().Should().Contain(eventoId.ToString());
        requestCapturado.Headers.Should().Contain(h => h.Key == "User-Agent");
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConRespuestaLenta_DeberiaCompletarseEventualmente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(100); // Simular latencia
                return responseMessage;
            });

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);
        stopwatch.Stop();

        // Assert
        resultado.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(90);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task EventoExisteYDisponibleAsync_ConMultiplesReintentos_DeberiaEventualmenteExitir(int numeroReintentos)
    {
        // Arrange
        var options = new VerificadorEventosOptions
        {
            BaseUrl = "http://localhost:5001",
            MaxRetries = 2
        };
        var mockOptions = new Mock<IOptions<VerificadorEventosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        
        // Re-crear el verificador para este test específico con reintentos habilitados
        // Nota: para que sea rápido, deberíamos ideally mockear el sleep duration provider,
        // pero aquí solo reducimos el número de reintentos.
        var verificador = new VerificadorEventosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);

        var eventoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        var contadorLlamadas = 0;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                contadorLlamadas++;
                if (contadorLlamadas < numeroReintentos)
                {
                    throw new HttpRequestException("Error temporal");
                }
                return Task.FromResult(responseMessage);
            });

        // Act
        var resultado = await verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeTrue();
        contadorLlamadas.Should().Be(numeroReintentos);
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConMultiplesErrores_DeberiaEventualmenteLanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        
        // Simular múltiples fallos consecutivos
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Servicio no disponible"));

        // Act & Assert - Múltiples llamadas deberían fallar
        for (int i = 0; i < 3; i++)
        {
            await FluentActions
                .Invoking(() => _verificador.EventoExisteYDisponibleAsync(eventoId))
                .Should()
                .ThrowAsync<ServicioExternoNoDisponibleException>();
        }
    }

    [Fact]
    public async Task EventoExisteYDisponibleAsync_ConRespuestaGrande_DeberiaProcessarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var largeContent = new string('x', 10000); // 10KB de contenido
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(largeContent, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.EventoExisteYDisponibleAsync(eventoId);

        // Assert
        resultado.Should().BeTrue();
    }
}