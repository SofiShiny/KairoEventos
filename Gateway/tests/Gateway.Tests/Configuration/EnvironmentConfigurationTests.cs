using FluentAssertions;
using Gateway.API.Configuration;
using Microsoft.Extensions.Configuration;

namespace Gateway.Tests.Configuration;

/// <summary>
/// Tests unitarios para la configuración de variables de entorno
/// Validates: Requirements 10.1, 10.2, 10.3, 10.4, 10.5
/// </summary>
public class EnvironmentConfigurationTests
{
    #region LoadConfiguration Tests

    [Fact]
    public void LoadConfiguration_ShouldReadKeycloakFromEnvironmentVariables()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak__Authority", "http://keycloak-env:8080/realms/Kairo" },
            { "Keycloak__Audience", "kairo-api-env" },
            { "Keycloak__MetadataAddress", "http://keycloak-env:8080/realms/Kairo/.well-known/openid-configuration" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);

        // Assert
        envConfig.Keycloak.Authority.Should().Be("http://keycloak-env:8080/realms/Kairo");
        envConfig.Keycloak.Audience.Should().Be("kairo-api-env");
        envConfig.Keycloak.MetadataAddress.Should().Be("http://keycloak-env:8080/realms/Kairo/.well-known/openid-configuration");
    }

    [Fact]
    public void LoadConfiguration_ShouldReadMicroservicesUrlsFromEnvironmentVariables()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Microservices__EventosUrl", "http://eventos-env:8080" },
            { "Microservices__AsientosUrl", "http://asientos-env:8080" },
            { "Microservices__UsuariosUrl", "http://usuarios-env:8080" },
            { "Microservices__EntradasUrl", "http://entradas-env:8080" },
            { "Microservices__ReportesUrl", "http://reportes-env:8080" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);

        // Assert
        envConfig.Microservices.EventosUrl.Should().Be("http://eventos-env:8080");
        envConfig.Microservices.AsientosUrl.Should().Be("http://asientos-env:8080");
        envConfig.Microservices.UsuariosUrl.Should().Be("http://usuarios-env:8080");
        envConfig.Microservices.EntradasUrl.Should().Be("http://entradas-env:8080");
        envConfig.Microservices.ReportesUrl.Should().Be("http://reportes-env:8080");
    }

    [Fact]
    public void LoadConfiguration_ShouldReadCorsOriginsFromEnvironmentVariables()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Cors__AllowedOrigins:0", "http://frontend1:3000" },
            { "Cors__AllowedOrigins:1", "http://frontend2:5173" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);

        // Assert
        envConfig.Cors.AllowedOrigins.Should().HaveCount(2);
        envConfig.Cors.AllowedOrigins.Should().Contain("http://frontend1:3000");
        envConfig.Cors.AllowedOrigins.Should().Contain("http://frontend2:5173");
    }

    [Fact]
    public void LoadConfiguration_ShouldFallbackToAppSettings_WhenEnvironmentVariablesNotSet()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak:Authority", "http://keycloak-appsettings:8080/realms/Kairo" },
            { "Keycloak:Audience", "kairo-api-appsettings" },
            { "Keycloak:MetadataAddress", "http://keycloak-appsettings:8080/realms/Kairo/.well-known/openid-configuration" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);

        // Assert
        envConfig.Keycloak.Authority.Should().Be("http://keycloak-appsettings:8080/realms/Kairo");
        envConfig.Keycloak.Audience.Should().Be("kairo-api-appsettings");
        envConfig.Keycloak.MetadataAddress.Should().Be("http://keycloak-appsettings:8080/realms/Kairo/.well-known/openid-configuration");
    }

    [Fact]
    public void LoadConfiguration_ShouldUseDefaults_WhenNoConfigurationProvided()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);

        // Assert
        envConfig.Keycloak.Authority.Should().Be("http://localhost:8180/realms/Kairo");
        envConfig.Keycloak.Audience.Should().Be("kairo-api");
        envConfig.Keycloak.MetadataAddress.Should().Be("http://localhost:8180/realms/Kairo/.well-known/openid-configuration");
        
        envConfig.Microservices.EventosUrl.Should().Be("http://localhost:5001");
        envConfig.Microservices.AsientosUrl.Should().Be("http://localhost:5002");
        envConfig.Microservices.UsuariosUrl.Should().Be("http://localhost:5003");
        envConfig.Microservices.EntradasUrl.Should().Be("http://localhost:5004");
        envConfig.Microservices.ReportesUrl.Should().Be("http://localhost:5005");
        
        envConfig.Cors.AllowedOrigins.Should().HaveCount(2);
        envConfig.Cors.AllowedOrigins.Should().Contain("http://localhost:5173");
        envConfig.Cors.AllowedOrigins.Should().Contain("http://localhost:3000");
    }

    [Fact]
    public void LoadConfiguration_ShouldPrioritizeEnvironmentVariables_OverAppSettings()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            // Environment variable (higher priority)
            { "Keycloak__Authority", "http://keycloak-env:8080/realms/Kairo" },
            // AppSettings (lower priority)
            { "Keycloak:Authority", "http://keycloak-appsettings:8080/realms/Kairo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);

        // Assert
        envConfig.Keycloak.Authority.Should().Be("http://keycloak-env:8080/realms/Kairo");
    }

    #endregion

    #region ValidateConfiguration Tests

    [Fact]
    public void ValidateConfiguration_ShouldPass_WhenAllRequiredFieldsArePresent()
    {
        // Arrange
        var envConfig = new EnvironmentConfiguration
        {
            Keycloak = new KeycloakSettings
            {
                Authority = "http://keycloak:8080/realms/Kairo",
                Audience = "kairo-api",
                MetadataAddress = "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
            },
            Microservices = new MicroservicesSettings
            {
                EventosUrl = "http://eventos:8080",
                AsientosUrl = "http://asientos:8080",
                UsuariosUrl = "http://usuarios:8080",
                EntradasUrl = "http://entradas:8080",
                ReportesUrl = "http://reportes:8080"
            },
            Cors = new CorsSettings
            {
                AllowedOrigins = new[] { "http://localhost:5173" }
            }
        };

        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateConfiguration_ShouldThrow_WhenKeycloakAuthorityIsMissing()
    {
        // Arrange
        var envConfig = new EnvironmentConfiguration
        {
            Keycloak = new KeycloakSettings
            {
                Authority = "", // Missing
                Audience = "kairo-api",
                MetadataAddress = "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
            },
            Microservices = new MicroservicesSettings
            {
                EventosUrl = "http://eventos:8080",
                AsientosUrl = "http://asientos:8080",
                UsuariosUrl = "http://usuarios:8080",
                EntradasUrl = "http://entradas:8080",
                ReportesUrl = "http://reportes:8080"
            },
            Cors = new CorsSettings
            {
                AllowedOrigins = new[] { "http://localhost:5173" }
            }
        };

        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak Authority*required*Keycloak__Authority*");
    }

    [Fact]
    public void ValidateConfiguration_ShouldThrow_WhenKeycloakAudienceIsMissing()
    {
        // Arrange
        var envConfig = new EnvironmentConfiguration
        {
            Keycloak = new KeycloakSettings
            {
                Authority = "http://keycloak:8080/realms/Kairo",
                Audience = "", // Missing
                MetadataAddress = "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
            },
            Microservices = new MicroservicesSettings
            {
                EventosUrl = "http://eventos:8080",
                AsientosUrl = "http://asientos:8080",
                UsuariosUrl = "http://usuarios:8080",
                EntradasUrl = "http://entradas:8080",
                ReportesUrl = "http://reportes:8080"
            },
            Cors = new CorsSettings
            {
                AllowedOrigins = new[] { "http://localhost:5173" }
            }
        };

        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak Audience*required*Keycloak__Audience*");
    }

    [Fact]
    public void ValidateConfiguration_ShouldThrow_WhenMicroserviceUrlIsMissing()
    {
        // Arrange
        var envConfig = new EnvironmentConfiguration
        {
            Keycloak = new KeycloakSettings
            {
                Authority = "http://keycloak:8080/realms/Kairo",
                Audience = "kairo-api",
                MetadataAddress = "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
            },
            Microservices = new MicroservicesSettings
            {
                EventosUrl = "", // Missing
                AsientosUrl = "http://asientos:8080",
                UsuariosUrl = "http://usuarios:8080",
                EntradasUrl = "http://entradas:8080",
                ReportesUrl = "http://reportes:8080"
            },
            Cors = new CorsSettings
            {
                AllowedOrigins = new[] { "http://localhost:5173" }
            }
        };

        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Eventos microservice URL*required*Microservices__EventosUrl*");
    }

    [Fact]
    public void ValidateConfiguration_ShouldThrow_WhenCorsOriginsAreEmpty()
    {
        // Arrange
        var envConfig = new EnvironmentConfiguration
        {
            Keycloak = new KeycloakSettings
            {
                Authority = "http://keycloak:8080/realms/Kairo",
                Audience = "kairo-api",
                MetadataAddress = "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration"
            },
            Microservices = new MicroservicesSettings
            {
                EventosUrl = "http://eventos:8080",
                AsientosUrl = "http://asientos:8080",
                UsuariosUrl = "http://usuarios:8080",
                EntradasUrl = "http://entradas:8080",
                ReportesUrl = "http://reportes:8080"
            },
            Cors = new CorsSettings
            {
                AllowedOrigins = Array.Empty<string>() // Empty
            }
        };

        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CORS origin*required*Cors__AllowedOrigins*");
    }

    [Fact]
    public void ValidateConfiguration_ShouldThrow_WithMultipleErrors_WhenMultipleFieldsAreMissing()
    {
        // Arrange
        var envConfig = new EnvironmentConfiguration
        {
            Keycloak = new KeycloakSettings
            {
                Authority = "", // Missing
                Audience = "", // Missing
                MetadataAddress = ""
            },
            Microservices = new MicroservicesSettings
            {
                EventosUrl = "", // Missing
                AsientosUrl = "http://asientos:8080",
                UsuariosUrl = "http://usuarios:8080",
                EntradasUrl = "http://entradas:8080",
                ReportesUrl = "http://reportes:8080"
            },
            Cors = new CorsSettings
            {
                AllowedOrigins = Array.Empty<string>() // Missing
            }
        };

        // Act
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Keycloak Authority*")
            .WithMessage("*Keycloak Audience*")
            .WithMessage("*Eventos microservice*")
            .WithMessage("*CORS origin*");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void LoadAndValidateConfiguration_ShouldWork_WithCompleteEnvironmentVariables()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak__Authority", "http://keycloak:8080/realms/Kairo" },
            { "Keycloak__Audience", "kairo-api" },
            { "Keycloak__MetadataAddress", "http://keycloak:8080/realms/Kairo/.well-known/openid-configuration" },
            { "Microservices__EventosUrl", "http://eventos:8080" },
            { "Microservices__AsientosUrl", "http://asientos:8080" },
            { "Microservices__UsuariosUrl", "http://usuarios:8080" },
            { "Microservices__EntradasUrl", "http://entradas:8080" },
            { "Microservices__ReportesUrl", "http://reportes:8080" },
            { "Cors__AllowedOrigins:0", "http://localhost:5173" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().NotThrow();
        envConfig.Keycloak.Authority.Should().Be("http://keycloak:8080/realms/Kairo");
        envConfig.Microservices.EventosUrl.Should().Be("http://eventos:8080");
        envConfig.Cors.AllowedOrigins.Should().Contain("http://localhost:5173");
    }

    [Fact]
    public void LoadAndValidateConfiguration_ShouldWork_WithDefaultValues()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var envConfig = ConfigurationLoader.LoadConfiguration(configuration);
        Action act = () => ConfigurationLoader.ValidateConfiguration(envConfig);

        // Assert
        act.Should().NotThrow();
        envConfig.Keycloak.Authority.Should().Be("http://localhost:8180/realms/Kairo");
        envConfig.Microservices.EventosUrl.Should().Be("http://localhost:5001");
        envConfig.Cors.AllowedOrigins.Should().HaveCount(2);
    }

    #endregion
}
