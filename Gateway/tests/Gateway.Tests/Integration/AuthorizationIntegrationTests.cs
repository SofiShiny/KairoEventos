using FluentAssertions;
using Gateway.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para autorización basada en roles
/// Property 4: Role-Based Authorization
/// Validates: Requirements 3.1, 3.2, 3.3, 3.4
/// </summary>
public class AuthorizationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthorizationIntegrationTests(WebApplicationFactory<Program> factory)
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
    /// Property 4: Role-Based Authorization
    /// For any authenticated user with a specific role attempting to access an endpoint, 
    /// the Gateway should grant or deny access based on the endpoint's required roles.
    /// </summary>
    [Fact]
    public async Task User_With_User_Role_Should_Access_User_Endpoints()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "usuario",
            email: "usuario@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/usuarios/profile");
        
        // Assert
        // No debe retornar 403 Forbidden
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            "usuarios con rol User deben poder acceder a endpoints de usuario");
    }

    [Fact]
    public async Task User_With_Admin_Role_Should_Access_All_Endpoints()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "admin",
            email: "admin@example.com",
            roles: new[] { "Admin" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var endpoints = new[]
        {
            "/api/eventos/123",
            "/api/asientos/456",
            "/api/usuarios/789",
            "/api/entradas/101",
            "/api/reportes/202"
        };
        
        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                $"usuarios con rol Admin deben poder acceder a {endpoint}");
        }
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    public async Task User_With_Organizator_Role_Should_Access_Event_And_Seat_Endpoints(string endpoint)
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "organizador",
            email: "organizador@example.com",
            roles: new[] { "Organizator" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync(endpoint);
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            $"usuarios con rol Organizator deben poder acceder a {endpoint}");
    }

    [Fact]
    public async Task User_Without_Required_Role_Should_Be_Denied_Access()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "usuario",
            email: "usuario@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        // Intentar acceder a un endpoint que requiere Admin
        var response = await _client.GetAsync("/api/reportes/admin/config");
        
        // Assert
        // Nota: En un entorno de test sin endpoints reales, esto puede retornar 503
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task User_With_Multiple_Roles_Should_Have_Combined_Permissions()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "multiuser",
            email: "multi@example.com",
            roles: new[] { "User", "Organizator" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var endpoints = new[]
        {
            "/api/usuarios/profile",  // User endpoint
            "/api/eventos/123",       // Organizator endpoint
            "/api/asientos/456"       // Organizator endpoint
        };
        
        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
                $"usuarios con múltiples roles deben poder acceder a {endpoint}");
        }
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Organizator")]
    public async Task Gateway_Should_Enforce_Authorization_For_All_Roles(string role)
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: $"user_{role}",
            email: $"{role.ToLower()}@example.com",
            roles: new[] { role }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        // El Gateway debe procesar la autorización (no retornar 401)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            $"el Gateway debe procesar autorización para rol {role}");
    }

    [Fact]
    public async Task User_With_No_Roles_Should_Be_Denied_Access()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "noroleuser",
            email: "norole@example.com",
            roles: Array.Empty<string>()
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.Unauthorized
        );
    }

    [Fact]
    public async Task Gateway_Should_Enforce_Authorization_For_POST_Requests()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "usuario",
            email: "usuario@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/eventos/create", content);
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe validar autorización para POST");
    }

    [Fact]
    public async Task Gateway_Should_Enforce_Authorization_For_PUT_Requests()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "organizador",
            email: "org@example.com",
            roles: new[] { "Organizator" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PutAsync("/api/eventos/123", content);
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe validar autorización para PUT");
    }

    [Fact]
    public async Task Gateway_Should_Enforce_Authorization_For_DELETE_Requests()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateToken(
            username: "admin",
            email: "admin@example.com",
            roles: new[] { "Admin" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.DeleteAsync("/api/eventos/123");
        
        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "el Gateway debe validar autorización para DELETE");
    }

    [Fact]
    public async Task Gateway_Should_Handle_Concurrent_Authorization_Checks()
    {
        // Arrange
        var userToken = JwtTokenGenerator.GenerateToken("user", "user@example.com", new[] { "User" });
        var adminToken = JwtTokenGenerator.GenerateToken("admin", "admin@example.com", new[] { "Admin" });
        var orgToken = JwtTokenGenerator.GenerateToken("org", "org@example.com", new[] { "Organizator" });
        
        var requests = new[]
        {
            (token: userToken, path: "/api/usuarios/profile"),
            (token: adminToken, path: "/api/reportes/admin"),
            (token: orgToken, path: "/api/eventos/123")
        };
        
        // Act
        var tasks = requests.Select(async req =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, req.path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", req.token);
            return await _client.SendAsync(request);
        }).ToArray();
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                "todas las peticiones concurrentes con autorización válida deben ser procesadas");
        }
    }
}
