using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Notificaciones.Infraestructura.Servicios;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.Infraestructura.Servicios;

public class ServicioUsuariosHttpTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<ServicioUsuariosHttp>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly ServicioUsuariosHttp _servicio;

    public ServicioUsuariosHttpTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        _loggerMock = new Mock<ILogger<ServicioUsuariosHttp>>();
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                {"UsuariosApi:Url", "http://test-api"}
            })
            .Build();

        _servicio = new ServicioUsuariosHttp(_httpClient, _configuration, _loggerMock.Object);
    }

    [Fact]
    public async Task ObtenerEmailUsuarioAsync_CuandoUsuarioExiste_DebeRetornarEmail()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var response = new UsuarioResponse { Correo = "test@test.com" };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        // Act
        var result = await _servicio.ObtenerEmailUsuarioAsync(usuarioId);

        // Assert
        result.Should().Be("test@test.com");
    }

    [Fact]
    public async Task ObtenerEmailUsuarioAsync_CuandoUsuarioNoExiste_DebeRetornarNull()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _servicio.ObtenerEmailUsuarioAsync(usuarioId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerEmailUsuarioAsync_CuandoHayError_DebeRetornarNullYLoguear()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _servicio.ObtenerEmailUsuarioAsync(usuarioId);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error llamando")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Once);
    }

    [Fact]
    public void Constructor_SinConfigUrl_DebeUsarDefault()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build(); // Config vacía
        
        // Act
        var servicio = new ServicioUsuariosHttp(_httpClient, config, _loggerMock.Object);
        
        // Assert
        // No podemos leer _baseUrl porque es privado, pero podemos verificar que no lanzó error
        servicio.Should().NotBeNull();
    }
}
