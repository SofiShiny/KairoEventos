using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pagos.Dominio.Interfaces;
using Pagos.Infraestructura;
using Xunit;

namespace Pagos.Pruebas.Infraestructura;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfraestructura_RegistraServiciosCorrectamente()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=test;Username=postgres;Password=postgres",
                ["RabbitMq:Host"] = "localhost"
            })
            .Build();

        // Act
        services.AddInfraestructura(configuration);
        services.AddLogging(); // Required by some services
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IRepositorioTransacciones>().Should().NotBeNull();
        provider.GetService<IPasarelaPago>().Should().NotBeNull();
        provider.GetService<IGeneradorFactura>().Should().NotBeNull();
        provider.GetService<IAlmacenadorArchivos>().Should().NotBeNull();
    }
}
