using FluentAssertions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Tests.Configuration;

/// <summary>
/// Tests unitarios para la configuración de autenticación JWT con Keycloak
/// Validates: Requirements 2.1, 2.5
/// </summary>
public class AuthenticationConfigurationTests
{
    [Fact]
    public void AddKeycloakAuthentication_ShouldRegisterJwtBearerAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authenticationSchemeProvider = serviceProvider
            .GetService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        
        authenticationSchemeProvider.Should().NotBeNull();
        
        var scheme = authenticationSchemeProvider!
            .GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme)
            .GetAwaiter()
            .GetResult();
        
        scheme.Should().NotBeNull();
        scheme!.Name.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureTokenValidationParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        jwtBearerOptions.Should().NotBeNull();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.TokenValidationParameters.Should().NotBeNull();
        options.TokenValidationParameters.ValidateIssuer.Should().BeTrue();
        options.TokenValidationParameters.ValidateAudience.Should().BeTrue();
        options.TokenValidationParameters.ValidateLifetime.Should().BeTrue();
        options.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureRoleClaimType()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.TokenValidationParameters.RoleClaimType.Should().Be("roles");
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureNameClaimType()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.TokenValidationParameters.NameClaimType.Should().Be("preferred_username");
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureAuthorityAndAudience()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.Authority.Should().Be("http://localhost:8180/realms/Kairo");
        options.Audience.Should().Be("kairo-api");
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureValidIssuerAndAudience()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.TokenValidationParameters.ValidIssuer.Should().Be("http://localhost:8180/realms/Kairo");
        options.TokenValidationParameters.ValidAudience.Should().Be("kairo-api");
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldThrowException_WhenAuthorityIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak:Audience", "kairo-api" }
            // Authority is missing
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        Action act = () => services.AddKeycloakAuthentication(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Authority*missing*");
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldThrowException_WhenAudienceIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            { "Keycloak:Authority", "http://localhost:8180/realms/Kairo" }
            // Audience is missing
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        Action act = () => services.AddKeycloakAuthentication(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Audience*missing*");
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldDisableHttpsMetadata_ForDevelopment()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.RequireHttpsMetadata.Should().BeFalse();
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureClockSkew()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.TokenValidationParameters.ClockSkew.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void AddKeycloakAuthentication_ShouldConfigureJwtBearerEvents()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddKeycloakAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtBearerOptions = serviceProvider
            .GetService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
        
        var options = jwtBearerOptions!.Get(JwtBearerDefaults.AuthenticationScheme);
        
        options.Events.Should().NotBeNull();
        options.Events.OnAuthenticationFailed.Should().NotBeNull();
        options.Events.OnTokenValidated.Should().NotBeNull();
        options.Events.OnChallenge.Should().NotBeNull();
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
