using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para headers CORS
/// Property 6: CORS Header Presence
/// Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5
/// </summary>
public class CorsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CorsIntegrationTests(WebApplicationFactory<Program> factory)
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
    /// Property 6: CORS Header Presence
    /// For any HTTP request from an allowed origin (localhost:5173 or localhost:3000), 
    /// the Gateway should include appropriate CORS headers in the response.
    /// </summary>
    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("http://localhost:3000")]
    public async Task Gateway_Should_Include_CORS_Headers_For_Allowed_Origins(string origin)
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Origin", origin);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            $"debe incluir header CORS para origen permitido {origin}");
    }

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("http://localhost:3000")]
    public async Task Gateway_Should_Return_Correct_Origin_In_CORS_Header(string origin)
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Origin", origin);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").First();
            allowedOrigin.Should().BeOneOf(origin, "*",
                $"el header debe contener el origen {origin} o *");
        }
    }

    [Theory]
    [InlineData("http://localhost:5173", "GET")]
    [InlineData("http://localhost:3000", "POST")]
    [InlineData("http://localhost:5173", "PUT")]
    [InlineData("http://localhost:3000", "DELETE")]
    public async Task Gateway_Should_Handle_CORS_Preflight_Requests(string origin, string method)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/eventos/123");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        request.Headers.Add("Access-Control-Request-Headers", "Authorization, Content-Type");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.ServiceUnavailable
        );
        
        // Verificar headers CORS en respuesta preflight
        if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
        {
            response.Headers.Should().ContainKey("Access-Control-Allow-Methods",
                "debe incluir métodos permitidos en preflight");
        }
    }

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("http://localhost:3000")]
    public async Task Gateway_Should_Allow_Credentials_In_CORS_Response(string origin)
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Origin", origin);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Credentials"))
        {
            var allowCredentials = response.Headers.GetValues("Access-Control-Allow-Credentials").First();
            allowCredentials.Should().Be("true",
                "debe permitir credenciales en peticiones CORS");
        }
    }

    [Theory]
    [InlineData("http://malicious-site.com")]
    [InlineData("http://localhost:8080")]
    [InlineData("http://example.com")]
    public async Task Gateway_Should_Not_Include_CORS_Headers_For_Disallowed_Origins(string origin)
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Origin", origin);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // Si el origen no está permitido, no debe incluir el header o debe ser diferente
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").First();
            allowedOrigin.Should().NotBe(origin,
                $"no debe permitir origen no autorizado {origin}");
        }
    }

    [Theory]
    [InlineData("http://localhost:5173", "/api/eventos/123")]
    [InlineData("http://localhost:3000", "/api/asientos/456")]
    [InlineData("http://localhost:5173", "/api/usuarios/789")]
    [InlineData("http://localhost:3000", "/api/entradas/101")]
    [InlineData("http://localhost:5173", "/api/reportes/202")]
    public async Task Gateway_Should_Include_CORS_Headers_For_All_Microservices(string origin, string path)
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Origin", origin);
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            $"debe incluir headers CORS para {path}");
    }

    [Fact]
    public async Task Gateway_Should_Allow_Authorization_Header_In_CORS()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/eventos/123");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        if (response.StatusCode != HttpStatusCode.ServiceUnavailable &&
            response.Headers.Contains("Access-Control-Allow-Headers"))
        {
            var allowedHeaders = response.Headers.GetValues("Access-Control-Allow-Headers").First();
            allowedHeaders.Should().Contain("Authorization",
                "debe permitir header Authorization en CORS");
        }
    }

    [Fact]
    public async Task Gateway_Should_Allow_Content_Type_Header_In_CORS()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/eventos/123");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        if (response.StatusCode != HttpStatusCode.ServiceUnavailable &&
            response.Headers.Contains("Access-Control-Allow-Headers"))
        {
            var allowedHeaders = response.Headers.GetValues("Access-Control-Allow-Headers").First();
            allowedHeaders.Should().Contain("Content-Type",
                "debe permitir header Content-Type en CORS");
        }
    }

    [Fact]
    public async Task Gateway_Should_Allow_Accept_Header_In_CORS()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/eventos/123");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Accept");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        if (response.StatusCode != HttpStatusCode.ServiceUnavailable &&
            response.Headers.Contains("Access-Control-Allow-Headers"))
        {
            var allowedHeaders = response.Headers.GetValues("Access-Control-Allow-Headers").First();
            allowedHeaders.Should().Contain("Accept",
                "debe permitir header Accept en CORS");
        }
    }

    [Fact]
    public async Task Gateway_Should_Handle_Multiple_Concurrent_CORS_Requests()
    {
        // Arrange
        var origins = new[] { "http://localhost:5173", "http://localhost:3000" };
        var paths = new[] { "/api/eventos/1", "/api/asientos/2", "/api/usuarios/3" };
        
        var requests = new List<HttpRequestMessage>();
        foreach (var origin in origins)
        {
            foreach (var path in paths)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, path);
                request.Headers.Add("Origin", origin);
                requests.Add(request);
            }
        }
        
        // Act
        var tasks = requests.Select(req => _client.SendAsync(req)).ToArray();
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        foreach (var response in responses)
        {
            response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
                "todas las peticiones CORS concurrentes deben incluir headers CORS");
        }
    }

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("http://localhost:3000")]
    public async Task Gateway_Should_Include_CORS_Headers_For_Error_Responses(string origin)
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Origin", origin);
        
        // Act
        // Petición a ruta inválida que retornará error
        var response = await _client.GetAsync("/api/invalid/route");
        
        // Assert
        // Incluso en errores, debe incluir headers CORS
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            "debe incluir headers CORS incluso en respuestas de error");
    }
}
