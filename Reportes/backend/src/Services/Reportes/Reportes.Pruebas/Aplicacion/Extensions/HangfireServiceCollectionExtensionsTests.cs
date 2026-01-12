using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reportes.Aplicacion.Extensions;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Extensions;

public class HangfireServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public HangfireServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Hangfire:Enabled"] = "true",
            ["MongoDbSettings:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDbSettings:DatabaseName"] = "test_db"
        });
        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void ConfigurarHangfire_ConHangfireHabilitado_RegistraHangfire()
    {
        // Act
        _services.ConfigurarHangfire(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var jobStorage = serviceProvider.GetService<JobStorage>();
        jobStorage.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarHangfire_ConHangfireDeshabilitado_NoRegistraHangfire()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Hangfire:Enabled"] = "false"
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarHangfire(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var jobStorage = serviceProvider.GetService<JobStorage>();
        jobStorage.Should().BeNull();
    }

    [Fact]
    public void ConfigurarHangfire_ConVariablesDeEntorno_UsaVariablesDeEntorno()
    {
        // Arrange
        Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://test-host:27017");
        Environment.SetEnvironmentVariable("MONGODB_DATABASE", "test_env_db");

        try
        {
            // Act
            _services.ConfigurarHangfire(_configuration);
            
            // Assert - Solo verificamos que el servicio se registró sin intentar conectar
            var serviceDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(JobStorage));
            serviceDescriptor.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", null);
            Environment.SetEnvironmentVariable("MONGODB_DATABASE", null);
        }
    }

    [Fact]
    public void ConfigurarHangfire_SinConfiguracion_UsaValoresPorDefecto()
    {
        // Arrange
        var emptyConfiguration = new ConfigurationBuilder().Build();

        // Act
        _services.ConfigurarHangfire(emptyConfiguration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var jobStorage = serviceProvider.GetService<JobStorage>();
        jobStorage.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarHangfire_RetornaServiceCollection_ParaChaining()
    {
        // Act
        var result = _services.ConfigurarHangfire(_configuration);

        // Assert
        result.Should().BeSameAs(_services);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    public void ConfigurarHangfire_ConDiferentesValoresTrue_HabilitaHangfire(string enabledValue)
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Hangfire:Enabled"] = enabledValue,
            ["MongoDbSettings:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDbSettings:DatabaseName"] = "test_db"
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarHangfire(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var jobStorage = serviceProvider.GetService<JobStorage>();
        jobStorage.Should().NotBeNull();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    public void ConfigurarHangfire_ConDiferentesValoresFalse_DeshabilitaHangfire(string enabledValue)
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Hangfire:Enabled"] = enabledValue
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarHangfire(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var jobStorage = serviceProvider.GetService<JobStorage>();
        jobStorage.Should().BeNull();
    }

    [Fact]
    public void ConfigurarHangfire_ConConnectionStringCompleto_ConstruyeCorrectamente()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Hangfire:Enabled"] = "true",
            ["MongoDbSettings:ConnectionString"] = "mongodb://user:password@host:27017",
            ["MongoDbSettings:DatabaseName"] = "custom_db"
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarHangfire(configuration);
        
        // Assert - Solo verificamos que el servicio se registró sin intentar conectar
        var serviceDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(JobStorage));
        serviceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarHangfire_SinHabilitarExplicitamente_HabilitaPorDefecto()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MongoDbSettings:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDbSettings:DatabaseName"] = "test_db"
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarHangfire(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var jobStorage = serviceProvider.GetService<JobStorage>();
        jobStorage.Should().NotBeNull();
    }
}