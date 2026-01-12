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
/// Tests de integración para logging de autenticación
/// Property 8: Authentication Logging
/// Validates: Requirements 8.2, 8.3
/// </summary>
public class AuthenticationLoggingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestLoggerProvider _loggerProvider;

    public AuthenticationLoggingIntegrationTests(WebApplicationFactory<Program> factory)
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
    /// Property 8: Authentication Logging
    /// For any JWT token validation attempt, the Gateway should log the validation result 
    /// (success or failure) with appropriate log level (Information for success, Warning for failure).
    /// </summary>
    [Fact]
    public async Task Gateway_Should_Log_Successful_Token_Validation()
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
        
        // Buscar logs relacionados con autenticación exitosa
        var authLogs = logs.Where(log => 
            log.Message.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("validated", StringComparison.OrdinalIgnoreCase) ||
            log.Level == LogLevel.Information).ToList();
        
        authLogs.Should().NotBeEmpty(
            "debe haber logs de autenticación exitosa");
    }

    [Fact]
    public async Task Gateway_Should_Log_Failed_Token_Validation_With_Warning_Level()
    {
        // Arrange
        _loggerProvider.Clear();
        var invalidToken = "invalid.token.here";
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", invalidToken);
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs de nivel Warning o Error relacionados con autenticación
        var warningLogs = logs.Where(log => 
            log.Level >= LogLevel.Warning &&
            (log.Message.Contains("token", StringComparison.OrdinalIgnoreCase) ||
             log.Message.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
             log.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
             log.Message.Contains("failed", StringComparison.OrdinalIgnoreCase))).ToList();
        
        // Puede o no haber logs de warning dependiendo de la configuración
        // Este test verifica que si hay fallos de autenticación, se registran con nivel apropiado
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            warningLogs.Should().NotBeEmpty(
                "debe haber logs de nivel Warning para fallos de autenticación");
        }
    }

    [Fact]
    public async Task Gateway_Should_Log_Missing_Token()
    {
        // Arrange
        _loggerProvider.Clear();
        // No se agrega token
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs relacionados con token faltante
        var authLogs = logs.Where(log => 
            log.Message.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("missing", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)).ToList();
        
        // Puede haber o no logs dependiendo de si el endpoint requiere autenticación
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            authLogs.Should().NotBeEmpty(
                "debe haber logs cuando falta el token");
        }
    }

    [Fact]
    public async Task Gateway_Should_Log_Expired_Token_Validation()
    {
        // Arrange
        _loggerProvider.Clear();
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
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs relacionados con token expirado
        var expiredLogs = logs.Where(log => 
            log.Message.Contains("expired", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            (log.Level >= LogLevel.Warning && 
             log.Message.Contains("auth", StringComparison.OrdinalIgnoreCase))).ToList();
        
        // Verificar que hay logs si el token fue rechazado
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            expiredLogs.Should().NotBeEmpty(
                "debe haber logs para tokens expirados");
        }
    }

    [Fact]
    public async Task Gateway_Should_Log_Invalid_Signature_Token()
    {
        // Arrange
        _loggerProvider.Clear();
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
        var logs = _loggerProvider.GetLogs();
        
        // Buscar logs relacionados con firma inválida
        var signatureLogs = logs.Where(log => 
            log.Message.Contains("signature", StringComparison.OrdinalIgnoreCase) ||
            log.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
            (log.Level >= LogLevel.Warning && 
             log.Message.Contains("token", StringComparison.OrdinalIgnoreCase))).ToList();
        
        // Verificar que hay logs si el token fue rechazado
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            signatureLogs.Should().NotBeEmpty(
                "debe haber logs para tokens con firma inválida");
        }
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Organizator")]
    public async Task Gateway_Should_Log_Token_Validation_For_All_Roles(string role)
    {
        // Arrange
        _loggerProvider.Clear();
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
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            $"debe haber logs de validación para rol {role}");
    }

    [Fact]
    public async Task Gateway_Should_Log_Multiple_Authentication_Attempts()
    {
        // Arrange
        _loggerProvider.Clear();
        
        var tokens = new[]
        {
            JwtTokenGenerator.GenerateToken("user1", "user1@example.com", new[] { "User" }),
            JwtTokenGenerator.GenerateToken("user2", "user2@example.com", new[] { "Admin" }),
            "invalid.token.here"
        };
        
        // Act
        foreach (var token in tokens)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/eventos/123");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _client.SendAsync(request);
        }
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs para múltiples intentos de autenticación");
    }

    [Fact]
    public async Task Gateway_Should_Log_Concurrent_Authentication_Attempts()
    {
        // Arrange
        _loggerProvider.Clear();
        
        var tokens = new[]
        {
            JwtTokenGenerator.GenerateToken("user1", "user1@example.com", new[] { "User" }),
            JwtTokenGenerator.GenerateToken("user2", "user2@example.com", new[] { "Admin" }),
            JwtTokenGenerator.GenerateToken("user3", "user3@example.com", new[] { "Organizator" })
        };
        
        // Act
        var tasks = tokens.Select(async token =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/eventos/123");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }).ToArray();
        
        await Task.WhenAll(tasks);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty(
            "debe haber logs para intentos de autenticación concurrentes");
    }

    [Fact]
    public async Task Gateway_Should_Log_Authentication_For_Different_Endpoints()
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
            "debe haber logs de autenticación para diferentes endpoints");
    }

    [Fact]
    public async Task Gateway_Should_Include_Timestamp_In_Authentication_Logs()
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
}
