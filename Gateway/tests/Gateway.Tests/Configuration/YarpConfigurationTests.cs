using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Gateway.Tests.Configuration;

/// <summary>
/// Tests unitarios para la configuración de rutas YARP
/// Valida Requirements 1.1, 1.2, 1.3, 1.4, 1.5
/// </summary>
public class YarpConfigurationTests
{
    private readonly IConfiguration _configuration;

    public YarpConfigurationTests()
    {
        // Cargar configuración desde appsettings.json
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();
    }

    [Fact]
    public void Configuration_Should_Have_ReverseProxy_Section()
    {
        // Arrange & Act
        var reverseProxySection = _configuration.GetSection("ReverseProxy");

        // Assert
        reverseProxySection.Exists().Should().BeTrue("la configuración debe tener una sección ReverseProxy");
    }

    [Theory]
    [InlineData("eventos-route")]
    [InlineData("asientos-route")]
    [InlineData("usuarios-route")]
    [InlineData("entradas-route")]
    [InlineData("reportes-route")]
    public void Configuration_Should_Have_All_Required_Routes(string routeId)
    {
        // Arrange & Act
        var route = _configuration.GetSection($"ReverseProxy:Routes:{routeId}");

        // Assert
        route.Exists().Should().BeTrue($"la ruta {routeId} debe estar definida");
    }

    [Theory]
    [InlineData("eventos-route", "/api/eventos/{**catch-all}")]
    [InlineData("asientos-route", "/api/asientos/{**catch-all}")]
    [InlineData("usuarios-route", "/api/usuarios/{**catch-all}")]
    [InlineData("entradas-route", "/api/entradas/{**catch-all}")]
    [InlineData("reportes-route", "/api/reportes/{**catch-all}")]
    public void Route_Should_Have_Correct_Path_Pattern(string routeId, string expectedPath)
    {
        // Arrange & Act
        var path = _configuration[$"ReverseProxy:Routes:{routeId}:Match:Path"];

        // Assert
        path.Should().Be(expectedPath, $"la ruta {routeId} debe tener el path correcto");
    }

    [Theory]
    [InlineData("eventos-route", "eventos-cluster")]
    [InlineData("asientos-route", "asientos-cluster")]
    [InlineData("usuarios-route", "usuarios-cluster")]
    [InlineData("entradas-route", "entradas-cluster")]
    [InlineData("reportes-route", "reportes-cluster")]
    public void Route_Should_Have_Correct_ClusterId(string routeId, string expectedClusterId)
    {
        // Arrange & Act
        var clusterId = _configuration[$"ReverseProxy:Routes:{routeId}:ClusterId"];

        // Assert
        clusterId.Should().Be(expectedClusterId, $"la ruta {routeId} debe apuntar al cluster correcto");
    }

    [Theory]
    [InlineData("eventos-route")]
    [InlineData("asientos-route")]
    [InlineData("usuarios-route")]
    [InlineData("entradas-route")]
    [InlineData("reportes-route")]
    public void Route_Should_Have_Path_Transform(string routeId)
    {
        // Arrange & Act
        var transform = _configuration[$"ReverseProxy:Routes:{routeId}:Transforms:0:PathPattern"];

        // Assert
        transform.Should().Be("/api/{**catch-all}", $"la ruta {routeId} debe tener la transformación de path correcta");
    }

    [Theory]
    [InlineData("eventos-cluster")]
    [InlineData("asientos-cluster")]
    [InlineData("usuarios-cluster")]
    [InlineData("entradas-cluster")]
    [InlineData("reportes-cluster")]
    public void Configuration_Should_Have_All_Required_Clusters(string clusterId)
    {
        // Arrange & Act
        var cluster = _configuration.GetSection($"ReverseProxy:Clusters:{clusterId}");

        // Assert
        cluster.Exists().Should().BeTrue($"el cluster {clusterId} debe estar definido");
    }

    [Theory]
    [InlineData("eventos-cluster")]
    [InlineData("asientos-cluster")]
    [InlineData("usuarios-cluster")]
    [InlineData("entradas-cluster")]
    [InlineData("reportes-cluster")]
    public void Cluster_Should_Have_At_Least_One_Destination(string clusterId)
    {
        // Arrange & Act
        var destinations = _configuration.GetSection($"ReverseProxy:Clusters:{clusterId}:Destinations");

        // Assert
        destinations.Exists().Should().BeTrue($"el cluster {clusterId} debe tener al menos un destino");
        destinations.GetChildren().Should().NotBeEmpty($"el cluster {clusterId} debe tener al menos un destino configurado");
    }

    [Theory]
    [InlineData("eventos-cluster", "destination1")]
    [InlineData("asientos-cluster", "destination1")]
    [InlineData("usuarios-cluster", "destination1")]
    [InlineData("entradas-cluster", "destination1")]
    [InlineData("reportes-cluster", "destination1")]
    public void Cluster_Destination_Should_Have_Valid_Address(string clusterId, string destinationId)
    {
        // Arrange & Act
        var address = _configuration[$"ReverseProxy:Clusters:{clusterId}:Destinations:{destinationId}:Address"];

        // Assert
        address.Should().NotBeNullOrEmpty($"el destino {destinationId} del cluster {clusterId} debe tener una dirección válida");
        address.Should().StartWith("http", $"la dirección debe ser una URL HTTP válida");
    }

    [Fact]
    public void All_Routes_Should_Have_Corresponding_Clusters()
    {
        // Arrange
        var routes = _configuration.GetSection("ReverseProxy:Routes").GetChildren();
        var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        var clusterIds = clusters.Select(c => c.Key).ToList();

        // Act & Assert
        foreach (var route in routes)
        {
            var clusterId = route["ClusterId"];
            clusterId.Should().NotBeNullOrEmpty($"la ruta {route.Key} debe tener un ClusterId");
            clusterIds.Should().Contain(clusterId, $"el cluster {clusterId} referenciado por la ruta {route.Key} debe existir");
        }
    }

    [Fact]
    public void Configuration_Should_Have_Exactly_Five_Routes()
    {
        // Arrange & Act
        var routes = _configuration.GetSection("ReverseProxy:Routes").GetChildren();

        // Assert
        routes.Should().HaveCount(5, "debe haber exactamente 5 rutas configuradas (eventos, asientos, usuarios, entradas, reportes)");
    }

    [Fact]
    public void Configuration_Should_Have_Exactly_Five_Clusters()
    {
        // Arrange & Act
        var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren();

        // Assert
        clusters.Should().HaveCount(5, "debe haber exactamente 5 clusters configurados (eventos, asientos, usuarios, entradas, reportes)");
    }
}
