using Eventos.Infraestructura;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Infraestructura;

// ========== Pruebas de ServiceCollectionExtensionsInfraTests.cs ==========

public class ServiceCollectionExtensionsInfraTests
{
    [Fact]
    public void AddEventoInfrastructureServices_SinConnectionString_Lanza()
    {
        var services = new ServiceCollection();
        var configMock = new Mock<IConfiguration>();
        var sectionMock = new Mock<IConfigurationSection>();
        // No configurar clave "EventosDb" para que GetConnectionString devuelva null
        configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(sectionMock.Object);
        var act = () => services.AddEventoInfrastructureServices(configMock.Object);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddEventoInfrastructureServices_ConConnectionString_RegistraServicios()
    {
        var services = new ServiceCollection();
        var configMock = new Mock<IConfiguration>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["EventosDb"]).Returns("Host=localhost;Database=test;Username=u;Password=p");
        configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(sectionMock.Object);

        services.AddEventoInfrastructureServices(configMock.Object);
        var provider = services.BuildServiceProvider();
        provider.GetService<Eventos.Infraestructura.Persistencia.EventosDbContext>().Should().NotBeNull();
        provider.GetService<Eventos.Dominio.Repositorios.IRepositorioEvento>().Should().NotBeNull();
    }
}
