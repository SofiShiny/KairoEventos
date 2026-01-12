using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para configuración por variables de entorno
/// Property 10: Environment Variable Configuration
/// Validates: Requirements 10.1, 10.2, 10.3, 10.4
/// </summary>
public class EnvironmentConfigurationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EnvironmentConfigurationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Property 10: Environment Variable Configuration
    /// For any required configuration value (Keycloak URL, microservice URLs, CORS origins), 
    /// the Gateway should read the value from environment variables if present, 
    /// otherwise use default values for development.
    /// </summary>
    [Fact]
    public void Gateway_Should_Read_Keycloak_URL_From_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var keycloakAuthority = configuration?["Keycloak:Authority"];
        
        // Assert
        keycloakAuthority.Should().NotBeNullOrEmpty(
            "debe leer la URL de Keycloak desde configuración");
        keycloakAuthority.Should().Contain("keycloak",
            "la URL debe contener 'keycloak'");
    }

    [Fact]
    public void Gateway_Should_Read_Keycloak_Audience_From_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var audience = configuration?["Keycloak:Audience"];
        
        // Assert
        audience.Should().NotBeNullOrEmpty(
            "debe leer el audience de Keycloak desde configuración");
        audience.Should().Be("kairo-api",
            "el audience debe ser 'kairo-api'");
    }

    [Fact]
    public void Gateway_Should_Read_Keycloak_Metadata_Address_From_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var metadataAddress = configuration?["Keycloak:MetadataAddress"];
        
        // Assert
        metadataAddress.Should().NotBeNullOrEmpty(
            "debe leer la dirección de metadata de Keycloak desde configuración");
        metadataAddress.Should().Contain(".well-known/openid-configuration",
            "debe ser una URL de metadata de OpenID Connect");
    }

    [Theory]
    [InlineData("eventos-cluster")]
    [InlineData("asientos-cluster")]
    [InlineData("usuarios-cluster")]
    [InlineData("entradas-cluster")]
    [InlineData("reportes-cluster")]
    public void Gateway_Should_Read_Microservice_URLs_From_Configuration(string clusterId)
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var address = configuration?[$"ReverseProxy:Clusters:{clusterId}:Destinations:destination1:Address"];
        
        // Assert
        address.Should().NotBeNullOrEmpty(
            $"debe leer la URL del microservicio {clusterId} desde configuración");
        address.Should().StartWith("http",
            "la URL debe comenzar con http");
    }

    [Fact]
    public void Gateway_Should_Read_CORS_Origins_From_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var corsOrigins = configuration?.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        // Assert
        corsOrigins.Should().NotBeNull("debe leer los orígenes CORS desde configuración");
        corsOrigins.Should().NotBeEmpty("debe haber al menos un origen CORS configurado");
        corsOrigins.Should().Contain(origin => origin.Contains("localhost"),
            "debe incluir orígenes localhost para desarrollo");
    }

    [Fact]
    public void Gateway_Should_Have_Default_CORS_Origins_For_Development()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var corsOrigins = configuration?.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        // Assert
        corsOrigins.Should().Contain("http://localhost:5173",
            "debe incluir puerto 5173 para Vite");
        corsOrigins.Should().Contain("http://localhost:3000",
            "debe incluir puerto 3000 para React");
    }

    [Fact]
    public async Task Gateway_Should_Use_Configuration_For_Routing()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var client = factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/eventos/123");
        
        // Assert
        // El Gateway debe usar la configuración para enrutar
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "debe usar la configuración de rutas");
    }

    [Fact]
    public void Gateway_Should_Load_All_Required_Configuration_Sections()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act & Assert
        configuration.GetSection("Keycloak").Exists().Should().BeTrue(
            "debe tener sección Keycloak");
        configuration.GetSection("Cors").Exists().Should().BeTrue(
            "debe tener sección Cors");
        configuration.GetSection("ReverseProxy").Exists().Should().BeTrue(
            "debe tener sección ReverseProxy");
    }

    [Fact]
    public void Gateway_Should_Have_Valid_Keycloak_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var authority = configuration?["Keycloak:Authority"];
        var audience = configuration?["Keycloak:Audience"];
        var metadataAddress = configuration?["Keycloak:MetadataAddress"];
        
        // Assert
        authority.Should().NotBeNullOrEmpty("Authority es requerido");
        audience.Should().NotBeNullOrEmpty("Audience es requerido");
        metadataAddress.Should().NotBeNullOrEmpty("MetadataAddress es requerido");
        
        authority.Should().StartWith("http", "Authority debe ser una URL");
        metadataAddress.Should().StartWith("http", "MetadataAddress debe ser una URL");
    }

    [Fact]
    public void Gateway_Should_Have_Valid_YARP_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false);
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var routes = configuration?.GetSection("ReverseProxy:Routes").GetChildren();
        var clusters = configuration?.GetSection("ReverseProxy:Clusters").GetChildren();
        
        // Assert
        routes.Should().NotBeNull("debe tener rutas configuradas");
        routes.Should().NotBeEmpty("debe haber al menos una ruta");
        clusters.Should().NotBeNull("debe tener clusters configurados");
        clusters.Should().NotBeEmpty("debe haber al menos un cluster");
    }

    [Fact]
    public void Gateway_Should_Support_Environment_Variable_Override()
    {
        // Arrange
        var customAuthority = "http://custom-keycloak:8080/realms/Custom";
        
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Keycloak:Authority"] = customAuthority
                });
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var authority = configuration?["Keycloak:Authority"];
        
        // Assert
        authority.Should().Be(customAuthority,
            "debe poder sobrescribir configuración con variables de entorno");
    }

    [Fact]
    public void Gateway_Should_Support_Multiple_CORS_Origins()
    {
        // Arrange
        var customOrigins = new[] 
        { 
            "http://localhost:5173", 
            "http://localhost:3000",
            "http://custom-origin:8080"
        };
        
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cors:AllowedOrigins:0"] = customOrigins[0],
                    ["Cors:AllowedOrigins:1"] = customOrigins[1],
                    ["Cors:AllowedOrigins:2"] = customOrigins[2]
                });
            });
        });
        
        var configuration = factory.Services.GetService(typeof(IConfiguration)) as IConfiguration;
        
        // Act
        var corsOrigins = configuration?.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        // Assert
        corsOrigins.Should().BeEquivalentTo(customOrigins,
            "debe soportar múltiples orígenes CORS");
    }
}
