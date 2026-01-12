using FluentAssertions;
using Gateway.API.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gateway.Tests.Configuration;

/// <summary>
/// Tests unitarios para la configuración de CORS
/// Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5
/// </summary>
public class CorsConfigurationTests
{
    [Fact]
    public void AddCorsPolicy_ShouldRegisterCorsServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging required by CORS
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsService = serviceProvider.GetService<ICorsService>();
        corsService.Should().NotBeNull();
    }

    [Fact]
    public void AddCorsPolicy_ShouldRegisterPolicyWithCorrectName()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        corsOptions.Should().NotBeNull();
        
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        policy.Should().NotBeNull();
    }

    [Fact]
    public void AddCorsPolicy_ShouldConfigureAllowedOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.Origins.Should().Contain("http://localhost:5173");
        policy.Origins.Should().Contain("http://localhost:3000");
    }

    [Fact]
    public void AddCorsPolicy_ShouldAllowAnyHeader()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.AllowAnyHeader.Should().BeTrue();
    }

    [Fact]
    public void AddCorsPolicy_ShouldAllowAnyMethod()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.AllowAnyMethod.Should().BeTrue();
    }

    [Fact]
    public void AddCorsPolicy_ShouldAllowCredentials()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.SupportsCredentials.Should().BeTrue();
    }

    [Fact]
    public void AddCorsPolicy_ShouldUseDefaultOrigin_WhenConfigurationIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.Origins.Should().Contain("http://localhost:5173");
    }

    [Fact]
    public void AddCorsPolicy_ShouldConfigureMultipleOrigins()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.Origins.Should().HaveCount(2);
    }

    [Fact]
    public void AddCorsPolicy_PolicyName_ShouldBeAllowFrontends()
    {
        // Arrange & Act & Assert
        CorsConfiguration.PolicyName.Should().Be("AllowFrontends");
    }

    [Fact]
    public void AddCorsPolicy_ShouldConfigurePolicy_WithAllRequiredSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        
        // Verificar todas las configuraciones requeridas
        policy!.Origins.Should().NotBeEmpty();
        policy.AllowAnyHeader.Should().BeTrue();
        policy.AllowAnyMethod.Should().BeTrue();
        policy.SupportsCredentials.Should().BeTrue();
    }

    [Fact]
    public void AddCorsPolicy_ShouldReadOriginsFromConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customOrigins = new[] { "http://example.com", "https://example.com" };
        var configData = new Dictionary<string, string?>
        {
            { "Cors:AllowedOrigins:0", customOrigins[0] },
            { "Cors:AllowedOrigins:1", customOrigins[1] }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        policy!.Origins.Should().Contain(customOrigins[0]);
        policy.Origins.Should().Contain(customOrigins[1]);
    }

    [Fact]
    public void AddCorsPolicy_ShouldNotAllowWildcardOrigin()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddCorsPolicy(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var corsOptions = serviceProvider.GetService<IOptions<CorsOptions>>();
        var policy = corsOptions!.Value.GetPolicy(CorsConfiguration.PolicyName);
        
        policy.Should().NotBeNull();
        // Cuando se usa AllowCredentials, no se puede usar wildcard origin (*)
        policy!.Origins.Should().NotContain("*");
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            { "Cors:AllowedOrigins:0", "http://localhost:5173" },
            { "Cors:AllowedOrigins:1", "http://localhost:3000" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }
}
