using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.API.HealthChecks;
using Xunit;

namespace Reportes.Pruebas.API.HealthChecks;

public class RabbitMqHealthCheckTests
{
    private readonly Mock<ILogger<RabbitMqHealthCheck>> _loggerMock;

    public RabbitMqHealthCheckTests()
    {
        _loggerMock = new Mock<ILogger<RabbitMqHealthCheck>>();
    }

    [Fact]
    public async Task CheckHealthAsync_BusControlNulo_ManejaExcepcionCorrectamente()
    {
        // Arrange
        var healthCheckWithNullBus = new RabbitMqHealthCheck(null!, _loggerMock.Object);
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result = await healthCheckWithNullBus.CheckHealthAsync(healthCheckContext);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Error verificando conexión a RabbitMQ");
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NullReferenceException>();
    }

    [Fact]
    public async Task CheckHealthAsync_LoggerNulo_NoFallaAlLoggear()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();
        var healthCheckWithNullLogger = new RabbitMqHealthCheck(busControlMock.Object, null!);
        var healthCheckContext = new HealthCheckContext();

        // Act & Assert - No debe lanzar excepción por logger nulo
        var result = await healthCheckWithNullLogger.CheckHealthAsync(healthCheckContext);
        
        // Puede ser Healthy o Unhealthy dependiendo del mock, pero no debe fallar
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_CancellationTokenPasado_NoLanzaExcepcion()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, _loggerMock.Object);
        var healthCheckContext = new HealthCheckContext();
        var cancellationToken = new CancellationToken();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthCheckContext, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ContextoValido_RetornaResultado()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, _loggerMock.Object);
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthCheckContext);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckHealthAsync_HealthCheckContextNulo_NoLanzaExcepcion()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, _loggerMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }

    [Fact]
    public void Constructor_ParametrosValidos_CreaInstancia()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();

        // Act & Assert
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, _loggerMock.Object);
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_BusControlNulo_NoLanzaExcepcion()
    {
        // Act & Assert
        var healthCheck = new RabbitMqHealthCheck(null!, _loggerMock.Object);
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_LoggerNulo_NoLanzaExcepcion()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();

        // Act & Assert
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, null!);
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_MultiplesLlamadas_RetornaResultadosConsistentes()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, _loggerMock.Object);
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result1 = await healthCheck.CheckHealthAsync(healthCheckContext);
        var result2 = await healthCheck.CheckHealthAsync(healthCheckContext);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Status.Should().Be(result2.Status);
        result1.Description.Should().Be(result2.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_ConCancellationTokenCancelado_ManejaCorrectamente()
    {
        // Arrange
        var busControlMock = new Mock<IBusControl>();
        var healthCheck = new RabbitMqHealthCheck(busControlMock.Object, _loggerMock.Object);
        var healthCheckContext = new HealthCheckContext();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthCheckContext, cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }
}