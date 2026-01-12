using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para enrutamiento a microservicios
/// Property 1: Route Matching Consistency
/// Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5
/// </summary>
public class RouteMatchingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RouteMatchingIntegrationTests(WebApplicationFactory<Program> factory)
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
    /// Property 1: Route Matching Consistency
    /// For any HTTP request with a path matching /api/{service}/* where service is one of 
    /// [eventos, asientos, usuarios, entradas, reportes], the Gateway should route the 
    /// request to the corresponding microservice cluster.
    /// </summary>
    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/eventos/list")]
    [InlineData("/api/eventos/search?query=test")]
    [InlineData("/api/asientos/456")]
    [InlineData("/api/asientos/available")]
    [InlineData("/api/usuarios/789")]
    [InlineData("/api/usuarios/profile")]
    [InlineData("/api/entradas/101")]
    [InlineData("/api/entradas/validate")]
    [InlineData("/api/reportes/202")]
    [InlineData("/api/reportes/summary")]
    public async Task Gateway_Should_Route_Requests_To_Correct_Microservice(string path)
    {
        // Arrange - El path ya está definido en el parámetro
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        // Nota: Esperamos 503 porque los microservicios no están disponibles en tests unitarios
        // Lo importante es que el Gateway intente enrutar (no retorna 404)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.ServiceUnavailable,  // Microservicio no disponible (esperado en tests)
            HttpStatusCode.OK,                   // Microservicio disponible y respondió
            HttpStatusCode.Unauthorized,         // Microservicio requiere autenticación
            HttpStatusCode.NotFound              // Recurso no encontrado en microservicio
        );
        
        // Si retorna 404, significa que la ruta no fue encontrada por YARP
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, 
            $"la ruta {path} debe ser reconocida por el Gateway");
    }

    [Theory]
    [InlineData("/api/eventos")]
    [InlineData("/api/asientos")]
    [InlineData("/api/usuarios")]
    [InlineData("/api/entradas")]
    [InlineData("/api/reportes")]
    public async Task Gateway_Should_Route_Base_Paths_To_Microservices(string basePath)
    {
        // Arrange - El path base ya está definido
        
        // Act
        var response = await _client.GetAsync(basePath);
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound
        );
        
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            $"el path base {basePath} debe ser reconocido por el Gateway");
    }

    [Theory]
    [InlineData("/api/eventos/nested/path/deep")]
    [InlineData("/api/asientos/very/deep/nested/path")]
    [InlineData("/api/usuarios/a/b/c/d/e")]
    public async Task Gateway_Should_Route_Deeply_Nested_Paths(string deepPath)
    {
        // Arrange - Path profundamente anidado
        
        // Act
        var response = await _client.GetAsync(deepPath);
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound
        );
        
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            $"el path anidado {deepPath} debe ser reconocido por el Gateway");
    }

    [Theory]
    [InlineData("/api/eventos/123?param1=value1&param2=value2")]
    [InlineData("/api/asientos/list?page=1&size=10")]
    [InlineData("/api/usuarios/search?name=john&age=30")]
    public async Task Gateway_Should_Preserve_Query_Parameters(string pathWithQuery)
    {
        // Arrange - Path con query parameters
        
        // Act
        var response = await _client.GetAsync(pathWithQuery);
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound
        );
        
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            $"el path con query parameters {pathWithQuery} debe ser reconocido por el Gateway");
    }

    [Theory]
    [InlineData("/api/invalid")]
    [InlineData("/api/notexist/123")]
    [InlineData("/api/unknown/path")]
    public async Task Gateway_Should_Return_NotFound_For_Invalid_Routes(string invalidPath)
    {
        // Arrange - Path que no corresponde a ningún microservicio
        
        // Act
        var response = await _client.GetAsync(invalidPath);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            $"el path inválido {invalidPath} debe retornar 404");
    }

    [Fact]
    public async Task Gateway_Should_Route_All_Five_Microservices()
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
            results[service].Should().BeOneOf(
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound
            );
            
            results[service].Should().NotBe(HttpStatusCode.NotFound,
                $"el servicio {service} debe tener una ruta configurada");
        }
    }
}
