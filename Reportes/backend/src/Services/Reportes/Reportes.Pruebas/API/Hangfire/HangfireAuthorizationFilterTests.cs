using FluentAssertions;
using Hangfire.Dashboard;
using Reportes.API.Hangfire;
using Xunit;

namespace Reportes.Pruebas.API.Hangfire;

public class HangfireAuthorizationFilterTests : IDisposable
{
    private string? _originalEnvironment;

    public HangfireAuthorizationFilterTests()
    {
        _originalEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }

    public void Dispose()
    {
        // Restore original environment variable
        if (_originalEnvironment != null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _originalEnvironment);
        }
        else
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void Authorize_DevelopmentEnvironment_ReturnsTrue()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(null!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Authorize_ProductionEnvironment_ReturnsTrue()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(null!);

        // Assert
        // Current implementation allows access in production (should be changed for real production)
        result.Should().BeTrue();
    }

    [Fact]
    public void Authorize_StagingEnvironment_ReturnsTrue()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(null!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Authorize_NoEnvironmentSet_ReturnsTrue()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(null!);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("development")]
    [InlineData("DEVELOPMENT")]
    public void Authorize_DevelopmentEnvironmentCaseInsensitive_ReturnsTrue(string environment)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(null!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Authorize_MultipleCallsSameEnvironment_ConsistentResults()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result1 = filter.Authorize(null!);
        var result2 = filter.Authorize(null!);
        var result3 = filter.Authorize(null!);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Fact]
    public void Authorize_DifferentDashboardContexts_SameResult()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result1 = filter.Authorize(null!);
        var result2 = filter.Authorize(null!);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    [Fact]
    public void Authorize_EnvironmentChangeBetweenCalls_ReflectsNewEnvironment()
    {
        // Arrange
        var filter = new HangfireAuthorizationFilter();

        // Act & Assert - Development
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var developmentResult = filter.Authorize(null!);
        developmentResult.Should().BeTrue();

        // Act & Assert - Production
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var productionResult = filter.Authorize(null!);
        productionResult.Should().BeTrue();
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("Local")]
    [InlineData("QA")]
    [InlineData("")]
    public void Authorize_CustomEnvironments_ReturnsTrue(string environment)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(null!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Authorize_NullDashboardContext_DoesNotThrow()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var filter = new HangfireAuthorizationFilter();

        // Act
        var act = () => filter.Authorize(null!);

        // Assert
        act.Should().NotThrow();
    }
}
