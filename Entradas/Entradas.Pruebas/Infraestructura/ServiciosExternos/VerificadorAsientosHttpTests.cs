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
/// Pruebas comprehensivas para VerificadorAsientosHttp
/// Incluye circuit breaker, políticas de resiliencia y manejo de errores
/// </summary>
public class VerificadorAsientosHttpTests
{
    private readonly Mock<ILogger<VerificadorAsientosHttp>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly VerificadorAsientosOptions _options;
    private readonly VerificadorAsientosHttp _verificador;

    public VerificadorAsientosHttpTests()
    {
        _mockLogger = new Mock<ILogger<VerificadorAsientosHttp>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5002")
        };
        
        _options = new VerificadorAsientosOptions
        {
            BaseUrl = "http://localhost:5002",
            TimeoutSeconds = 1,
            MaxRetries = 0, // Desactivar por defecto para velocidad
            CircuitBreakerFailureThreshold = 2,
            CircuitBreakerDurationSeconds = 1
        };

        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        _verificador = new VerificadorAsientosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConAsientoDisponible_DeberiaRetornarTrue()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConAsientoIdNulo_DeberiaRetornarTrue()
    {
        // Arrange
        var eventoId = Guid.NewGuid();

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, null);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConAsientoNoDisponible_DeberiaRetornarFalse()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConEventoIdVacio_DeberiaLanzarArgumentException()
    {
        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.AsientoDisponibleAsync(Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("El ID del evento no puede ser vacío*");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConParametrosValidos_DeberiaEjecutarSinError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConAsientoYaReservado_DeberiaLanzarAsientoNoDisponibleException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .ThrowAsync<AsientoNoDisponibleException>()
            .WithMessage($"El asiento {asientoId} ya está reservado");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConDuracionInvalida_DeberiaLanzarArgumentException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.Zero;

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("La duración debe ser mayor a cero*");
    }

    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaCrearInstancia()
    {
        // Arrange & Act
        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        var verificador = new VerificadorAsientosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);

        // Assert
        verificador.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConHttpClientNulo_DeberiaLanzarArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        // Act & Assert
        FluentActions
            .Invoking(() => new VerificadorAsientosHttp(null!, _mockLogger.Object, mockOptions.Object))
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DeberiaLanzarArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(_options);

        // Act & Assert
        FluentActions
            .Invoking(() => new VerificadorAsientosHttp(_httpClient, null!, mockOptions.Object))
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ConOptionsNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        FluentActions
            .Invoking(() => new VerificadorAsientosHttp(_httpClient, _mockLogger.Object, null!))
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task AsientoDisponibleAsync_ConErroresHttp5xx_DeberiaRetornarFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(statusCode);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Gone)]
    public async Task AsientoDisponibleAsync_ConErroresHttp4xx_DeberiaRetornarFalse(HttpStatusCode statusCode)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(statusCode);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConTimeout_DeberiaLanzarServicioExternoNoDisponibleException()
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
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.AsientoDisponibleAsync(eventoId, asientoId))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado*");
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConCancellationToken_DebeRespetarCancelacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
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
            .Invoking(() => _verificador.AsientoDisponibleAsync(eventoId, asientoId, cts.Token))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error inesperado*");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConErrorHttp_DeberiaLanzarServicioExternoNoDisponibleException()
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
            .ThrowsAsync(new HttpRequestException("Error de conexión"));

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .ThrowAsync<ServicioExternoNoDisponibleException>()
            .WithMessage("*Error de comunicación con el servicio de asientos*");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConAsientoIdVacio_DeberiaLanzarArgumentException()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.Empty;
        var duracion = TimeSpan.FromMinutes(15);

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("El ID del asiento no puede ser vacío*");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConEventoIdVacio_DeberiaLanzarArgumentException()
    {
        // Arrange
        var eventoId = Guid.Empty;
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("El ID del evento no puede ser vacío*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task ReservarAsientoTemporalAsync_ConDuracionNegativaOCero_DeberiaLanzarArgumentException(int minutos)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(minutos);

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("La duración debe ser mayor a cero*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(60)]
    [InlineData(1440)] // 24 horas
    public async Task ReservarAsientoTemporalAsync_ConDuracionesValidas_DeberiaEjecutarCorrectamente(int minutos)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(minutos);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConRespuestaJson_DeberiaProcessarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var asientoInfo = new { Id = asientoId, EventoId = eventoId, Disponible = true, Fila = 1, Numero = 15 };
        var jsonContent = JsonSerializer.Serialize(asientoInfo);
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
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConMultiplesLlamadasSimultaneas_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var tasks = asientoIds.Select(id => _verificador.AsientoDisponibleAsync(eventoId, id));
        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().AllSatisfy(resultado => resultado.Should().BeTrue());
        resultados.Should().HaveCount(10);
    }

    [Fact]
    public async Task AsientoDisponibleAsync_DeberiaEnviarHeadersCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
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
        await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        requestCapturado.Should().NotBeNull();
        requestCapturado!.Method.Should().Be(HttpMethod.Get);
        requestCapturado.RequestUri.Should().NotBeNull();
        requestCapturado.RequestUri!.ToString().Should().Contain(eventoId.ToString());
        requestCapturado.RequestUri.ToString().Should().Contain(asientoId.ToString());
        requestCapturado.Headers.Should().Contain(h => h.Key == "User-Agent");
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_DeberiaEnviarPostConDatosCorrectos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);
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
        await _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion);

        // Assert
        requestCapturado.Should().NotBeNull();
        requestCapturado!.Method.Should().Be(HttpMethod.Post);
        requestCapturado.Content.Should().NotBeNull();
        requestCapturado.Headers.Should().Contain(h => h.Key == "User-Agent");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task AsientoDisponibleAsync_ConMultiplesReintentos_DeberiaEventualmenteExitir(int numeroReintentos)
    {
        // Arrange
        var options = new VerificadorAsientosOptions
        {
            BaseUrl = "http://localhost:5002",
            MaxRetries = 2
        };
        var mockOptions = new Mock<IOptions<VerificadorAsientosOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        
        var verificador = new VerificadorAsientosHttp(_httpClient, _mockLogger.Object, mockOptions.Object);

        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
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
        var resultado = await verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeTrue();
        contadorLlamadas.Should().Be(numeroReintentos);
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConMultiplesErrores_DeberiaEventualmenteLanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        
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
                .Invoking(() => _verificador.AsientoDisponibleAsync(eventoId, asientoId))
                .Should()
                .ThrowAsync<ServicioExternoNoDisponibleException>();
        }
    }

    [Fact]
    public async Task ReservarAsientoTemporalAsync_ConMultiplesErrores_DeberiaEventualmenteLanzarExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);
        
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
                .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
                .Should()
                .ThrowAsync<ServicioExternoNoDisponibleException>();
        }
    }

    [Fact]
    public async Task AsientoDisponibleAsync_ConRespuestaGrande_DeberiaProcessarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
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
        var resultado = await _verificador.AsientoDisponibleAsync(eventoId, asientoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/xml")]
    [InlineData("text/plain")]
    public async Task ReservarAsientoTemporalAsync_ConDiferentesContentTypes_DeberiaFuncionarCorrectamente(string contentType)
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var duracion = TimeSpan.FromMinutes(15);
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

        // Act & Assert
        await FluentActions
            .Invoking(() => _verificador.ReservarAsientoTemporalAsync(eventoId, asientoId, Guid.NewGuid(), duracion))
            .Should()
            .NotThrowAsync();
    }
}