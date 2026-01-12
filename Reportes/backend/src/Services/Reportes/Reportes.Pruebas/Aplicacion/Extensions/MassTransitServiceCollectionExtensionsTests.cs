using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Consumers;
using Reportes.Aplicacion.Extensions;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Extensions;

public class MassTransitServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public MassTransitServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        AddMockDependencies(_services);
        
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MassTransit:Enabled"] = "true",
            ["RabbitMqSettings:Host"] = "localhost",
            ["RabbitMqSettings:Port"] = "5672",
            ["RabbitMqSettings:Username"] = "guest",
            ["RabbitMqSettings:Password"] = "guest"
        });
        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void ConfigurarMassTransit_ConMassTransitHabilitado_RegistraMassTransit()
    {
        // Act
        _services.ConfigurarMassTransit(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var busControl = serviceProvider.GetService<IBusControl>();
        busControl.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarMassTransit_ConMassTransitDeshabilitado_RegistraConsumersComoServicios()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MassTransit:Enabled"] = "false"
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarMassTransit(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var busControl = serviceProvider.GetService<IBusControl>();
        busControl.Should().BeNull();

        // Verificar que los consumers están registrados como servicios regulares
        var eventoPublicadoConsumer = serviceProvider.GetService<EventoPublicadoConsumer>();
        eventoPublicadoConsumer.Should().NotBeNull();

        var asistenteRegistradoConsumer = serviceProvider.GetService<AsistenteRegistradoConsumer>();
        asistenteRegistradoConsumer.Should().NotBeNull();

        var eventoCanceladoConsumer = serviceProvider.GetService<EventoCanceladoConsumer>();
        eventoCanceladoConsumer.Should().NotBeNull();

        var mapaAsientosCreadoConsumer = serviceProvider.GetService<MapaAsientosCreadoConsumer>();
        mapaAsientosCreadoConsumer.Should().NotBeNull();

        var asientoAgregadoConsumer = serviceProvider.GetService<AsientoAgregadoConsumer>();
        asientoAgregadoConsumer.Should().NotBeNull();

        var asientoReservadoConsumer = serviceProvider.GetService<AsientoReservadoConsumer>();
        asientoReservadoConsumer.Should().NotBeNull();

        var asientoLiberadoConsumer = serviceProvider.GetService<AsientoLiberadoConsumer>();
        asientoLiberadoConsumer.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarMassTransit_ConVariablesDeEntorno_UsaVariablesDeEntorno()
    {
        // Arrange
        Environment.SetEnvironmentVariable("RABBITMQ_HOST", "test-host");
        Environment.SetEnvironmentVariable("RABBITMQ_PORT", "5673");
        Environment.SetEnvironmentVariable("RABBITMQ_USER", "test-user");
        Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "test-password");

        try
        {
            // Act
            _services.ConfigurarMassTransit(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var busControl = serviceProvider.GetService<IBusControl>();
            busControl.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("RABBITMQ_HOST", null);
            Environment.SetEnvironmentVariable("RABBITMQ_PORT", null);
            Environment.SetEnvironmentVariable("RABBITMQ_USER", null);
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", null);
        }
    }

    [Fact]
    public void ConfigurarMassTransit_SinConfiguracion_UsaValoresPorDefecto()
    {
        // Arrange
        var emptyConfiguration = new ConfigurationBuilder().Build();

        // Act
        _services.ConfigurarMassTransit(emptyConfiguration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var busControl = serviceProvider.GetService<IBusControl>();
        busControl.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarMassTransit_RetornaServiceCollection_ParaChaining()
    {
        // Act
        var result = _services.ConfigurarMassTransit(_configuration);

        // Assert
        result.Should().BeSameAs(_services);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    public void ConfigurarMassTransit_ConDiferentesValoresTrue_HabilitaMassTransit(string enabledValue)
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MassTransit:Enabled"] = enabledValue
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarMassTransit(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var busControl = serviceProvider.GetService<IBusControl>();
        busControl.Should().NotBeNull();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    public void ConfigurarMassTransit_ConDiferentesValoresFalse_DeshabilitaMassTransit(string enabledValue)
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MassTransit:Enabled"] = enabledValue
        });
        var configuration = configurationBuilder.Build();

        // Act
        _services.ConfigurarMassTransit(configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var busControl = serviceProvider.GetService<IBusControl>();
        busControl.Should().BeNull();
    }

    [Fact]
    public void ConfigurarMassTransit_ConPuertoInvalido_UsaPuertoPorDefecto()
    {
        // Arrange
        Environment.SetEnvironmentVariable("RABBITMQ_PORT", "invalid-port");

        try
        {
            // Act
            _services.ConfigurarMassTransit(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var busControl = serviceProvider.GetService<IBusControl>();
            busControl.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("RABBITMQ_PORT", null);
        }
    }

    private static void AddMockDependencies(IServiceCollection services)
    {
        // Add mock dependencies that the consumers need
        var mockRepository = new Mock<IRepositorioReportesLectura>();
        services.AddSingleton(mockRepository.Object);
        
        // Add logging
        services.AddLogging();
    }
}