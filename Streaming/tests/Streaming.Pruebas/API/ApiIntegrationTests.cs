using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Streaming.Infraestructura.Persistencia;
using MassTransit;
using Xunit;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using FluentAssertions;

namespace Streaming.Pruebas.API;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                // Reemplazar DbContext con InMemory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StreamingDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<StreamingDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                // Reemplazar MassTransit con InMemory
                services.AddMassTransitTestHarness(x =>
                {
                    // No hace falta configurar mucho más aquí para una prueba básica de inicio
                });
            });
        });
    }

    [Fact]
    public async Task App_DebeIniciarYResponderSwagger()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task App_DebeResponderNoEncontradoParaEventoInexistente()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/Streaming/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
