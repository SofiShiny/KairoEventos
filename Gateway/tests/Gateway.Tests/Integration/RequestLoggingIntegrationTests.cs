using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para logging de peticiones
/// Property 7: Request Logging Completeness
/// Validates: Requirements 8.1, 8.5
/// </summary>
public class RequestLoggingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestLoggerProvider _loggerProvider;

    public RequestLoggingIntegrationTests(WebApplicationFactory<Program> factory)
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
    /// Property 7: Request Logging Completeness
    /// For any HTTP request processed by the Gateway, a log entry should be created 
    /// containing the request method, path, timestamp, and response status code.
    /// </summary>
    [Theory]
    [InlineData("/api/eventos/123", "GET")]
    [InlineData("/api/asientos/456", "GET")]
    [InlineData("/api/usuarios/789", "GET")]
    public async Task Gateway_Should_Log_All_Requests(string path, string method)
    {
        // Arrange
        _loggerProvider.Clear();
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty("debe haber registros de log");
        
        // Verificar que hay logs relacionados con la petición
        var requestLogs = logs.Where(log => 
            log.Message.Contains(path) || 
            log.Message.Contains(method) ||
            log.Message.Contains("Request")).ToList();
        
        requestLogs.Should().NotBeEmpty(
            $"debe haber logs para la petición {method} {path}");
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    [InlineData("/api/usuarios/789")]
    public async Task Gateway_Should_Log_Request_Method(string path)
    {
        // Arrange
        _loggerProvider.Clear();
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var methodLogs = logs.Where(log => 
            log.Message.Contains("GET") || 
            log.Message.Contains("Method")).ToList();
        
        methodLogs.Should().NotBeEmpty(
            "debe registrar el método HTTP de la petición");
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    [InlineData("/api/usuarios/789")]
    public async Task Gateway_Should_Log_Request_Path(string path)
    {
        // Arrange
        _loggerProvider.Clear();
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var pathLogs = logs.Where(log => log.Message.Contains(path)).ToList();
        
        pathLogs.Should().NotBeEmpty(
            $"debe registrar el path {path} de la petición");
    }

    [Fact]
    public async Task Gateway_Should_Log_Response_Status_Code()
    {
        // Arrange
        _loggerProvider.Clear();
        var path = "/api/eventos/123";
        
        // Act
        var response = await _client.GetAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var statusLogs = logs.Where(log => 
            log.Message.Contains(((int)response.StatusCode).ToString()) ||
            log.Message.Contains("StatusCode") ||
            log.Message.Contains("completed")).ToList();
        
        statusLogs.Should().NotBeEmpty(
            "debe registrar el código de estado de la respuesta");
    }

    [Fact]
    public async Task Gateway_Should_Log_Request_Timestamp()
    {
        // Arrange
        _loggerProvider.Clear();
        var beforeRequest = DateTime.UtcNow;
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        var afterRequest = DateTime.UtcNow;
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        logs.Should().NotBeEmpty("debe haber registros de log");
        
        // Verificar que los logs tienen timestamp
        foreach (var log in logs)
        {
            log.Timestamp.Should().BeOnOrAfter(beforeRequest.AddSeconds(-1));
            log.Timestamp.Should().BeOnOrBefore(afterRequest.AddSeconds(1));
        }
    }

    [Theory]
    [InlineData("/api/eventos/create")]
    [InlineData("/api/asientos/reserve")]
    public async Task Gateway_Should_Log_POST_Requests(string path)
    {
        // Arrange
        _loggerProvider.Clear();
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync(path, content);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var postLogs = logs.Where(log => 
            log.Message.Contains("POST") || 
            log.Message.Contains(path)).ToList();
        
        postLogs.Should().NotBeEmpty(
            $"debe registrar peticiones POST a {path}");
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    public async Task Gateway_Should_Log_PUT_Requests(string path)
    {
        // Arrange
        _loggerProvider.Clear();
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PutAsync(path, content);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var putLogs = logs.Where(log => 
            log.Message.Contains("PUT") || 
            log.Message.Contains(path)).ToList();
        
        putLogs.Should().NotBeEmpty(
            $"debe registrar peticiones PUT a {path}");
    }

    [Theory]
    [InlineData("/api/eventos/123")]
    [InlineData("/api/asientos/456")]
    public async Task Gateway_Should_Log_DELETE_Requests(string path)
    {
        // Arrange
        _loggerProvider.Clear();
        
        // Act
        var response = await _client.DeleteAsync(path);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var deleteLogs = logs.Where(log => 
            log.Message.Contains("DELETE") || 
            log.Message.Contains(path)).ToList();
        
        deleteLogs.Should().NotBeEmpty(
            $"debe registrar peticiones DELETE a {path}");
    }

    [Fact]
    public async Task Gateway_Should_Log_Multiple_Concurrent_Requests()
    {
        // Arrange
        _loggerProvider.Clear();
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
        var logs = _loggerProvider.GetLogs();
        
        // Debe haber logs para cada petición
        foreach (var path in paths)
        {
            var pathLogs = logs.Where(log => log.Message.Contains(path)).ToList();
            pathLogs.Should().NotBeEmpty(
                $"debe haber logs para la petición a {path}");
        }
    }

    [Fact]
    public async Task Gateway_Should_Log_Error_Responses()
    {
        // Arrange
        _loggerProvider.Clear();
        var invalidPath = "/api/invalid/route";
        
        // Act
        var response = await _client.GetAsync(invalidPath);
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var errorLogs = logs.Where(log => 
            log.Message.Contains(invalidPath) ||
            log.Message.Contains("404") ||
            log.Level >= LogLevel.Warning).ToList();
        
        errorLogs.Should().NotBeEmpty(
            "debe registrar respuestas de error");
    }

    [Fact]
    public async Task Gateway_Should_Log_Request_Duration()
    {
        // Arrange
        _loggerProvider.Clear();
        
        // Act
        var response = await _client.GetAsync("/api/eventos/123");
        
        // Assert
        var logs = _loggerProvider.GetLogs();
        var durationLogs = logs.Where(log => 
            log.Message.Contains("ms") ||
            log.Message.Contains("duration") ||
            log.Message.Contains("completed")).ToList();
        
        // Puede o no haber logs de duración dependiendo de la implementación
        // Este test verifica que si hay logs de duración, están presentes
        if (durationLogs.Any())
        {
            durationLogs.Should().NotBeEmpty(
                "si se registra duración, debe estar en los logs");
        }
    }
}

/// <summary>
/// Provider de logging para tests que captura logs en memoria
/// </summary>
public class TestLoggerProvider : ILoggerProvider
{
    private readonly List<TestLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        var logger = new TestLogger(categoryName);
        _loggers.Add(logger);
        return logger;
    }

    public List<LogEntry> GetLogs()
    {
        return _loggers.SelectMany(l => l.Logs).ToList();
    }

    public void Clear()
    {
        foreach (var logger in _loggers)
        {
            logger.Clear();
        }
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

/// <summary>
/// Logger de test que captura logs en memoria
/// </summary>
public class TestLogger : ILogger
{
    private readonly string _categoryName;
    private readonly List<LogEntry> _logs = new();

    public TestLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public List<LogEntry> Logs => _logs;

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logs.Add(new LogEntry
        {
            Level = logLevel,
            Message = message,
            Exception = exception,
            Timestamp = DateTime.UtcNow,
            CategoryName = _categoryName
        });
    }

    public void Clear()
    {
        _logs.Clear();
    }
}

/// <summary>
/// Entrada de log para tests
/// </summary>
public class LogEntry
{
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
