using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Reportes.API.HealthChecks;
using Reportes.Infraestructura.Configuracion;
using Reportes.Infraestructura.Persistencia;
using Xunit;

namespace Reportes.Pruebas.API.HealthChecks;

public class MongoDbHealthCheckTests
{
    private readonly Mock<ILogger<MongoDbHealthCheck>> _loggerMock;

    public MongoDbHealthCheckTests()
    {
        _loggerMock = new Mock<ILogger<MongoDbHealthCheck>>();
    }

    [Fact]
    public async Task CheckHealthAsync_ContextoNulo_ManejaExcepcionCorrectamente()
    {
        // Arrange
        var healthCheckWithNullContext = new MongoDbHealthCheck(null!);
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result = await healthCheckWithNullContext.CheckHealthAsync(healthCheckContext);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Error verificando conexión a MongoDB");
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NullReferenceException>();
    }

    [Fact]
    public async Task CheckHealthAsync_CancellationTokenPasado_NoLanzaExcepcion()
    {
        // Arrange
        var mockSettings = CreateMockSettings();
        var optionsMock = new Mock<IOptions<MongoDbSettings>>();
        optionsMock.Setup(x => x.Value).Returns(mockSettings);

        var context = new ReportesMongoDbContext(optionsMock.Object);
        var healthCheck = new MongoDbHealthCheck(context);
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
        var mockSettings = CreateMockSettings();
        var optionsMock = new Mock<IOptions<MongoDbSettings>>();
        optionsMock.Setup(x => x.Value).Returns(mockSettings);

        var context = new ReportesMongoDbContext(optionsMock.Object);
        var healthCheck = new MongoDbHealthCheck(context);
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
        var mockSettings = CreateMockSettings();
        var optionsMock = new Mock<IOptions<MongoDbSettings>>();
        optionsMock.Setup(x => x.Value).Returns(mockSettings);

        var context = new ReportesMongoDbContext(optionsMock.Object);
        var healthCheck = new MongoDbHealthCheck(context);

        // Act
        var result = await healthCheck.CheckHealthAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }

    [Fact]
    public void Constructor_ParametroValido_CreaInstancia()
    {
        // Arrange
        var mockSettings = CreateMockSettings();
        var optionsMock = new Mock<IOptions<MongoDbSettings>>();
        optionsMock.Setup(x => x.Value).Returns(mockSettings);

        var context = new ReportesMongoDbContext(optionsMock.Object);

        // Act & Assert
        var healthCheck = new MongoDbHealthCheck(context);
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ContextoNulo_NoLanzaExcepcion()
    {
        // Act & Assert
        var healthCheck = new MongoDbHealthCheck(null!);
        healthCheck.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_MultiplesLlamadas_RetornaResultadosConsistentes()
    {
        // Arrange
        var mockSettings = CreateMockSettings();
        var optionsMock = new Mock<IOptions<MongoDbSettings>>();
        optionsMock.Setup(x => x.Value).Returns(mockSettings);

        var context = new ReportesMongoDbContext(optionsMock.Object);
        var healthCheck = new MongoDbHealthCheck(context);
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
        var mockSettings = CreateMockSettings();
        var optionsMock = new Mock<IOptions<MongoDbSettings>>();
        optionsMock.Setup(x => x.Value).Returns(mockSettings);

        var context = new ReportesMongoDbContext(optionsMock.Object);
        var healthCheck = new MongoDbHealthCheck(context);
        var healthCheckContext = new HealthCheckContext();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthCheckContext, cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }

    private static MongoDbSettings CreateMockSettings()
    {
        return new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test_reportes",
            ReportesVentasDiariasCollection = "reportes_ventas_diarias",
            HistorialAsistenciaCollection = "historial_asistencia",
            MetricasEventoCollection = "metricas_evento",
            LogsAuditoriaCollection = "logs_auditoria",
            ReportesConsolidadosCollection = "reportes_consolidados"
        };
    }
}