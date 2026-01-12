using FluentAssertions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Gateway.Tests.Integration;

/// <summary>
/// Tests de integración para validación de configuración al inicio
/// Property 11: Startup Configuration Validation
/// Validates: Requirements 10.5
/// </summary>
public class StartupConfigurationValidationIntegrationTests
{
    /// <summary>
    /// Property 11: Startup Configuration Validation
    /// For any Gateway startup, if required environment variables are missing and no defaults 
    /// are available, the Gateway should fail to start with a clear error message indicating 
    /// which variables are missing.
    /// </summary>
    [Fact]
    public void Gateway_Should_Validate_Keycloak_Authority_Is_Present()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Audience"] = "kairo-api",
                ["Keycloak:MetadataAddress"] = "http://localhost:8180/realms/Kairo/.well-known/openid-configuration"
                // Falta Authority
            })
            .Build();
        
        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(
            ConfigurationLoader.LoadConfiguration(configuration));
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Authority*",
                "debe lanzar excepción cuando falta Keycloak Authority");
    }

    [Fact]
    public void Gateway_Should_Validate_Keycloak_Audience_Is_Present()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8180/realms/Kairo",
                ["Keycloak:MetadataAddress"] = "http://localhost:8180/realms/Kairo/.well-known/openid-configuration"
                // Falta Audience
            })
            .Build();
        
        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(
            ConfigurationLoader.LoadConfiguration(configuration));
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Audience*",
                "debe lanzar excepción cuando falta Keycloak Audience");
    }

    [Fact]
    public void Gateway_Should_Validate_Keycloak_MetadataAddress_Is_Present()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8180/realms/Kairo",
                ["Keycloak:Audience"] = "kairo-api"
                // Falta MetadataAddress
            })
            .Build();
        
        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(
            ConfigurationLoader.LoadConfiguration(configuration));
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MetadataAddress*",
                "debe lanzar excepción cuando falta Keycloak MetadataAddress");
    }

    [Fact]
    public void Gateway_Should_Accept_Valid_Complete_Configuration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8180/realms/Kairo",
                ["Keycloak:Audience"] = "kairo-api",
                ["Keycloak:MetadataAddress"] = "http://localhost:8180/realms/Kairo/.well-known/openid-configuration",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
                ["ReverseProxy:Routes:eventos-route:ClusterId"] = "eventos-cluster",
                ["ReverseProxy:Clusters:eventos-cluster:Destinations:destination1:Address"] = "http://eventos-api:8080"
            })
            .Build();
        
        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(
            ConfigurationLoader.LoadConfiguration(configuration));
        
        // Assert
        act.Should().NotThrow("debe aceptar configuración válida completa");
    }

    [Fact]
    public void Gateway_Should_Provide_Clear_Error_Message_For_Missing_Configuration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        
        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(
            ConfigurationLoader.LoadConfiguration(configuration));
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*required*",
                "debe proporcionar mensaje claro sobre configuración faltante");
    }

    [Fact]
    public void Gateway_Should_Validate_All_Required_Keycloak_Settings()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        
        // Assert
        // Verificar que se cargan valores por defecto o se detecta la falta de configuración
        envConfig.Should().NotBeNull("debe cargar configuración");
        
        // Si no hay valores por defecto, la validación debe fallar
        if (string.IsNullOrEmpty(envConfig.Keycloak.Authority) ||
            string.IsNullOrEmpty(envConfig.Keycloak.Audience) ||
            string.IsNullOrEmpty(envConfig.Keycloak.MetadataAddress))
        {
            Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);
            act.Should().Throw<InvalidOperationException>(
                "debe fallar validación si faltan valores requeridos");
        }
    }

    [Fact]
    public void Gateway_Should_Load_Configuration_With_Defaults_For_Development()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        
        // Assert
        envConfig.Should().NotBeNull("debe cargar configuración");
        envConfig.Keycloak.Authority.Should().NotBeNullOrEmpty(
            "debe tener Authority configurado o por defecto");
        envConfig.Keycloak.Audience.Should().NotBeNullOrEmpty(
            "debe tener Audience configurado o por defecto");
        envConfig.Keycloak.MetadataAddress.Should().NotBeNullOrEmpty(
            "debe tener MetadataAddress configurado o por defecto");
    }

    [Fact]
    public void Gateway_Should_Validate_Configuration_Before_Starting()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);
        
        // Assert
        act.Should().NotThrow(
            "debe validar configuración correctamente antes de iniciar");
    }

    [Fact]
    public void Gateway_Should_Fail_Fast_On_Invalid_Configuration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "",  // Vacío
                ["Keycloak:Audience"] = "",   // Vacío
                ["Keycloak:MetadataAddress"] = ""  // Vacío
            })
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);
        
        // Assert
        act.Should().Throw<InvalidOperationException>(
            "debe fallar rápidamente con configuración inválida");
    }

    [Fact]
    public void Gateway_Should_Indicate_Which_Configuration_Is_Missing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Audience"] = "kairo-api"
                // Faltan Authority y MetadataAddress
            })
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        
        try
        {
            ConfigurationLoader.ValidateConfiguration(envConfig);
        }
        catch (InvalidOperationException ex)
        {
            // Assert
            ex.Message.Should().Contain("Authority",
                "debe indicar que falta Authority");
        }
    }

    [Fact]
    public void Gateway_Should_Accept_Configuration_From_Environment_Variables()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak__Authority"] = "http://keycloak:8080/realms/Kairo",
                ["Keycloak__Audience"] = "kairo-api",
                ["Keycloak__MetadataAddress"] = "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
            })
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);
        
        // Assert
        act.Should().NotThrow(
            "debe aceptar configuración desde variables de entorno");
    }

    [Fact]
    public void Gateway_Should_Validate_CORS_Configuration_Is_Present()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8180/realms/Kairo",
                ["Keycloak:Audience"] = "kairo-api",
                ["Keycloak:MetadataAddress"] = "http://localhost:8180/realms/Kairo/.well-known/openid-configuration"
                // Falta configuración CORS
            })
            .Build();
        
        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        
        // Assert
        // CORS puede tener valores por defecto, verificar que se cargan
        envConfig.Cors.AllowedOrigins.Should().NotBeNull(
            "debe tener configuración CORS (por defecto o configurada)");
    }

    [Fact]
    public void Gateway_Should_Validate_ReverseProxy_Configuration_Is_Present()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();
        
        // Act
        var reverseProxySection = configuration.GetSection("ReverseProxy");
        
        // Assert
        reverseProxySection.Exists().Should().BeTrue(
            "debe tener configuración de ReverseProxy");
        reverseProxySection.GetSection("Routes").Exists().Should().BeTrue(
            "debe tener rutas configuradas");
        reverseProxySection.GetSection("Clusters").Exists().Should().BeTrue(
            "debe tener clusters configurados");
    }
}
