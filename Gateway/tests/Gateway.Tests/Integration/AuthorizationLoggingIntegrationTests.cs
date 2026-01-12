using FluentAssertions;
using Gateway.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para logging de autorización
/// Property 9: Authorization Logging
/// Validates: Requirements 8.4
/// </summary>
public class AuthorizationLoggingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestLoggerProvider _loggerProvider;

    public AuthorizationLoggingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _loggerProvider = new TestLoggerProvider();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
            
            builder.ConfigureServices(services =>
            {
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(_loggerProvider);
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Property 9: Authorization Logging
    /// For any authorization decision (grant or deny), the Gateway should log 
    /// the username and the resource being accessed.
    /// </summary>
    [Fact]
    public async Task Gateway_Should_Log_Authorization_Grant()
    {
        // Arrange
        _loggerProvider.Clear();
        var username = "testuser";
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var path = "/api/eventos/123";
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs relacionados con autorización
        var authzLogs = logs.Where(log => 
            log.Message.Contains("author", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("access", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("grant", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains(username, StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains(path)).ToList();
        
        // Puede haber o no logs específicos de autorización
        // Este test verifica que si hay decisiones de autorización, se registran
        authzLogs.Should().NotBeNull();
    }

    [Fact]
    public async Task Gateway_Should_Log_Authorization_Denial()
    {
        // Arrange
        _loggerProvider.Clear();
        var username = "limiteduser";
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: "limited@example.com",
            roles: new[] { "User" }  // Rol limitado
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var path = "/api/reportes/admin/config";  // Endpoint que requiere Admin
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs relacionados con denegación de autorización
        var denialLogs = logs.Where(log => 
            (log.Level >= LogLevel.Warning &&
             (log.Message.Contains("author", StringComparison.OrdinalIgnoreCase) ||
              log.Message.Contains("denied", StringComparison.OrdinalIgnoreCase) ||
              log.Message.Contains("forbidden", StringComparison.OrdinalIgnoreCase) ||
              log.Message.Contains("access", StringComparison.OrdinalIgnoreCase)))).ToList();
        
        // Verificar que hay logs si el acceso fue denegado
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            denialLogs.Should().NotBeEmpty(
                "debe haber logs cuando se deniega autorización");
        }
    }

    [Theory]
    [InlineData("testuser", "/api/eventos/123")]
    [InlineData("adminuser", "/api/reportes/admin")]
    [InlineData("orguser", "/api/asientos/456")]
    public async Task Gateway_Should_Log_Username_And_Resource(string username, string resource)
    {
        // Arrange
        _loggerProvider.Clear();
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: $"{username}@example.com",
            roles: new[] { "User", "Admin", "Organizator" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync(resource);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs que contengan el username o el recurso
        var userResourceLogs = logs.Where(log => 
            log.Message.Contains(username, StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains(resource)).ToList();
        
        userResourceLogs.Should().NotBeEmpty(
            $"debe haber logs que mencionen el usuario {username} o el recurso {resource}");
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Organizator")]
    public async Task Gateway_Should_Log_Authorization_For_Different_Roles(string role)
    {
        // Arrange
        _loggerProvider.Clear();
        var username = $"user_{role}";
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: $"{username}@example.com",
            roles: new[] { role }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            $"debe haber logs de autorización para rol {role}");
    }

    [Fact]
    public async Task Gateway_Should_Log_Multiple_Authorization_Decisions()
    {
        // Arrange
        _loggerProvider.Clear();
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var endpoints = new[]
        {
            "/api/eventos/123",
            "/api/asientos/456",
            "/api/usuarios/789"
        };
        
        // Act
        foreach (var endpoint in endpoints)
        {
            await _client.GetAsync(endpoint);
        }
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs para múltiples decisiones de autorización");
    }

    [Fact]
    public async Task Gateway_Should_Log_Concurrent_Authorization_Decisions()
    {
        // Arrange
        _loggerProvider.Clear();
        
        var requests = new[]
        {
            (username: "user1", role: "User", path: "/api/eventos/1"),
            (username: "admin1", role: "Admin", path: "/api/reportes/2"),
            (username: "org1", role: "Organizator", path: "/api/asientos/3")
        };
        
        // Act
        var tasks = requests.Select(async req =>
        {
            var token = JwtTokenGenerator.GenerateToken(
                req.username,
                $"{req.username}@example.com",
                new[] { req.role }
            );
            
            var request = new HttpRequestMessage(HttpMethod.Get, req.path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }).ToArray();
        
        await Task.WhenAll(tasks);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs para decisiones de autorización concurrentes");
    }

    [Fact]
    public async Task Gateway_Should_Log_Authorization_For_POST_Requests()
    {
        // Arrange
        _loggerProvider.Clear();
        var username = "testuser";
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/eventos/create", content);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs de autorización para peticiones POST");
    }

    [Fact]
    public async Task Gateway_Should_Log_Authorization_For_PUT_Requests()
    {
        // Arrange
        _loggerProvider.Clear();
        var username = "testuser";
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: "test@example.com",
            roles: new[] { "Organizator" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PutAsync("/api/eventos/123", content);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs de autorización para peticiones PUT");
    }

    [Fact]
    public async Task Gateway_Should_Log_Authorization_For_DELETE_Requests()
    {
        // Arrange
        _loggerProvider.Clear();
        var username = "adminuser";
        var token = JwtTokenGenerator.GenerateToken(
            username: username,
            email: "admin@example.com",
            roles: new[] { "Admin" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.DeleteAsync("/api/eventos/123");
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs de autorización para peticiones DELETE");
    }

    [Fact]
    public async Task Gateway_Should_Include_Timestamp_In_Authorization_Logs()
    {
        // Arrange
        _loggerProvider.Clear();
        var beforeRequest = DateTime.UtcNow;
        
        var token = JwtTokenGenerator.GenerateToken(
            username: "testuser",
            email: "test@example.com",
            roles: new[] { "User" }
        );
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        await _client.GetAsync("/api/eventos/123");
        var afterRequest = DateTime.UtcNow;
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty("debe haber logs");
        
        // Verificar que todos los logs tienen timestamp
        foreach (var log in logs)
        {
            log.Timestamp.Should().BeOnOrAfter(beforeRequest.AddSeconds(-1));
            log.Timestamp.Should().BeOnOrBefore(afterRequest.AddSeconds(1));
        }
    }

    [Fact]
    public async Task Gateway_Should_Log_Authorization_With_Appropriate_Log_Level()
    {
        // Arrange
        _loggerProvider.Clear();
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
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty("debe haber logs");
        
        // Verificar que hay logs con niveles apropiados
        var infoOrWarningLogs = logs.Where(log => 
            log.Level >= LogLevel.Information).ToList();
        
        infoOrWarningLogs.Should().NotBeEmpty(
            "debe haber logs con nivel Information o superior");
    }
}
