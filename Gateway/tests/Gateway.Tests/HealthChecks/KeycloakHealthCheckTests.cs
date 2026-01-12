using FluentAssertions;
using Gateway.API.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace Gateway.Tests.HealthChecks;

/// <summary>
/// Tests unitarios para KeycloakHealthCheck
/// Validates: Requirements 7.2, 7.3
/// </summary>
public class KeycloakHealthCheckTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<KeycloakHealthCheck>> _loggerMock;
    private readonly IConfiguration _configuration;

    public KeycloakHealthCheckTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<KeycloakHealthCheck>>();
        _configuration = CreateTestConfiguration();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenKeycloakIsReachable()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Keycloak is reachable");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnDegraded_WhenKeycloakReturnsNonSuccessStatusCode()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("status code");
        result.Description.Should().Contain("InternalServerError");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenKeycloakIsNotReachable()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Keycloak is not reachable");
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenRequestTimesOut()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Keycloak health check timed out");
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnDegraded_WhenMetadataAddressIsNotConfigured()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak:Authority", "http://localhost:8180/realms/Kairo" },
            { "Keycloak:Audience", "kairo-api" }
            // MetadataAddress is missing
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Be("Keycloak MetadataAddress is not configured");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldLogDebug_WhenHealthCheckSucceeds()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("succeeded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldLogWarning_WhenKeycloakReturnsNonSuccessStatusCode()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("status code")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldLogError_WhenHealthCheckFails()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldUseCorrectMetadataUrl()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri.Should().NotBeNull();
        capturedRequest.RequestUri!.ToString().Should()
            .Be("http://localhost:8180/realms/Kairo/.well-known/openid-configuration");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldSetTimeoutTo5Seconds()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        HttpClient? capturedClient = null;
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() =>
            {
                var client = new HttpClient(httpMessageHandlerMock.Object);
                capturedClient = client;
                return client;
            });

        var healthCheck = new KeycloakHealthCheck(
            _httpClientFactoryMock.Object,
            _configuration,
            _loggerMock.Object);

        var context = new HealthCheckContext();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert
        capturedClient.Should().NotBeNull();
        capturedClient!.Timeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak:Authority", "http://localhost:8180/realms/Kairo" },
            { "Keycloak:Audience", "kairo-api" },
            { "Keycloak:MetadataAddress", "http://localhost:8180/realms/Kairo/.well-known/openid-configuration" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }
}
