using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para manejo de servicios no disponibles
/// Property 2: Service Unavailability Handling
/// Validates: Requirements 1.6
/// </summary>
public class ServiceUnavailabilityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ServiceUnavailabilityIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Property 2: Service Unavailability Handling
    /// For any request to a microservice that is unavailable or unreachable, 
    /// the Gateway should return HTTP 503 Service Unavailable.
    /// </summary>
    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    [InlineData("/api/usuarios/789")]
    [InlineData("/api/entradas/101")]
    [InlineData("/api/reportes/202")]
    public async Task Gateway_Should_Return_503_When_Microservice_Is_Unavailable(string path)
    {
        // Arrange
        // Los microservicios no están disponibles en el entorno de test
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        // Esperamos 503 porque los microservicios no están corriendo
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            $"el Gateway debe retornar 503 cuando el microservicio no está disponible para {path}");
    }

    [Theory]
    [InlineData("/api/eventos")]
    [InlineData("/api/asientos")]
    [InlineData("/api/usuarios")]
    [InlineData("/api/entradas")]
    [InlineData("/api/reportes")]
    public async Task Gateway_Should_Return_503_For_All_Unavailable_Services(string basePath)
    {
        // Arrange
        // Todos los microservicios están no disponibles
        
        // Act
        var response = await _client.GetAsync(basePath);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            $"el Gateway debe retornar 503 para el servicio no disponible en {basePath}");
    }

    [Fact]
    public async Task Gateway_Should_Return_503_For_All_Five_Unavailable_Microservices()
    {
        // Arrange
        var services = new[] { "eventos", "asientos", "usuarios", "entradas", "reportes" };
        var results = new Dictionary<string, HttpStatusCode>();
        
        // Act
        foreach (var service in services)
        {
            var response = await _client.GetAsync($"/api/{service}/test");
            results[service] = response.StatusCode;
        }
        
        // Assert
        foreach (var service in services)
        {
            results[service].Should().Be(HttpStatusCode.ServiceUnavailable,
                $"el servicio {service} debe retornar 503 cuando no está disponible");
        }
    }

    [Theory]
    [InlineData("/api/eventos/create")]
    [InlineData("/api/asientos/reserve")]
    [InlineData("/api/usuarios/register")]
    public async Task Gateway_Should_Return_503_For_POST_Requests_To_Unavailable_Services(string path)
    {
        // Arrange
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync(path, content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            $"el Gateway debe retornar 503 para POST cuando el servicio no está disponible en {path}");
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    public async Task Gateway_Should_Return_503_For_PUT_Requests_To_Unavailable_Services(string path)
    {
        // Arrange
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PutAsync(path, content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            $"el Gateway debe retornar 503 para PUT cuando el servicio no está disponible en {path}");
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    public async Task Gateway_Should_Return_503_For_DELETE_Requests_To_Unavailable_Services(string path)
    {
        // Arrange - Path para DELETE
        
        // Act
        var response = await _client.DeleteAsync(path);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            $"el Gateway debe retornar 503 para DELETE cuando el servicio no está disponible en {path}");
    }

    [Fact]
    public async Task Gateway_Should_Handle_Multiple_Concurrent_Requests_To_Unavailable_Services()
    {
        // Arrange
        var paths = new[]
        {
            "/api/eventos/1",
            "/api/asientos/2",
            "/api/usuarios/3",
            "/api/entradas/4",
            "/api/reportes/5"
        };
        
        // Act
        var tasks = paths.Select(path => _client.GetAsync(path)).ToArray();
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
                "todas las peticiones concurrentes deben retornar 503");
        }
    }

    [Fact]
    public async Task Gateway_Should_Return_Error_Response_Body_For_Unavailable_Service()
    {
        // Arrange
        var path = "/api/eventos/123";
        
        // Act
        var response = await _client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        content.Should().NotBeNullOrEmpty("debe retornar un cuerpo de respuesta con información del error");
    }
}
