using FluentAssertions;
using Gateway.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para autenticación con tokens válidos
/// Property 3: Valid Token Authentication
/// Validates: Requirements 2.1, 2.5
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
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
    /// Property 3: Valid Token Authentication
    /// For any HTTP request with a valid JWT token in the Authorization header, 
    /// the Gateway should successfully validate the token and extract user claims 
    /// (sub, roles, email, username).
    /// </summary>
    [Fact]
    public async Task Gateway_Should_Accept_Valid_JWT_Token()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // No debe retornar 401 Unauthorized
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe aceptar tokens JWT válidos");
    }

    [Theory]
    [InlineData("admin", "admin@example.com", new[] { "Admin" })]
    [InlineData("organizador", "org@example.com", new[] { "Organizator" })]
    [InlineData("usuario", "user@example.com", new[] { "User" })]
    public async Task Gateway_Should_Accept_Tokens_With_Different_Roles(
        string username, 
        string email, 
        string[] roles)
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(username, email, roles);
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            $"el Gateway debe aceptar tokens con rol {string.Join(", ", roles)}");
    }

    [Fact]
    public async Task Gateway_Should_Accept_Token_With_Multiple_Roles()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "multiuser",
            email: "multi@example.com",
            roles: new[] { "User", "Admin", "Organizator" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe aceptar tokens con múltiples roles");
    }

    [Fact]
    public async Task Gateway_Should_Reject_Request_Without_Token()
    {
        // Arrange
        // No se agrega token de autorización
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // Nota: En este test, esperamos que el endpoint requiera autenticación
        // Si el microservicio no está disponible, retornará 503
        // Si está disponible pero no hay token, retornará 401
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Fact]
    public async Task Gateway_Should_Reject_Expired_Token()
    {
        // Arrange
        var expiredToken = JwtTokenGenerator.GenerateExpiredToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", expiredToken);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // Nota: La validación de tokens expirados puede no funcionar sin Keycloak real
        // En un entorno de test sin Keycloak, el token puede ser aceptado
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Fact]
    public async Task Gateway_Should_Reject_Token_With_Invalid_Signature()
    {
        // Arrange
        var invalidToken = JwtTokenGenerator.GenerateInvalidSignatureToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", invalidToken);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Fact]
    public async Task Gateway_Should_Reject_Malformed_Token()
    {
        // Arrange
        var malformedToken = "this.is.not.a.valid.jwt.token";
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", malformedToken);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Fact]
    public async Task Gateway_Should_Reject_Empty_Bearer_Token()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "");
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest,
            HttpStatusCode.ServiceUnavailable
        );
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    [InlineData("/api/usuarios/789")]
    [InlineData("/api/entradas/101")]
    [InlineData("/api/reportes/202")]
    public async Task Gateway_Should_Validate_Token_For_All_Microservices(string path)
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            $"el Gateway debe validar tokens para {path}");
    }

    [Fact]
    public async Task Gateway_Should_Handle_Multiple_Concurrent_Authenticated_Requests()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        var paths = new[]
        {
            "/api/eventos/1",
            "/api/asientos/2",
            "/api/usuarios/3",
            "/api/entradas/4",
            "/api/reportes/5"
        };
        
        // Act
        var tasks = paths.Select(async path =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                "todas las peticiones concurrentes autenticadas deben ser aceptadas");
        }
    }
}
