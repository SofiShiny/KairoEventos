using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pagos.Aplicacion.DTOs;
using Pagos.Dominio.Entidades;
using Pagos.Infraestructura.Persistencia;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Pagos.API;
using Hangfire;
using Hangfire.MemoryStorage;

namespace Pagos.Pruebas.API;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development"); // Cover IsDevelopment branch
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMq:Host"] = null // Cover ?? "localhost" branch
                });
            });
            builder.ConfigureServices(services =>
            {
                // Replace DbContext with InMemory
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PagosDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<PagosDbContext>(options => options.UseInMemoryDatabase("ApiTestDb"));

                // Override Hangfire with Memory Storage to avoid Postgres dependency
                GlobalConfiguration.Configuration.UseMemoryStorage();
                services.AddHangfire(config => config.UseMemoryStorage());

                // Mock MassTransit to avoid RabbitMQ connection
                services.AddMassTransitTestHarness();
            });
        });
    }

    [Fact]
    public async Task Post_Pagos_RetornaAccepted()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dto = new CrearPagoDto(Guid.NewGuid(), Guid.NewGuid(), 100, "1234123412340000");

        // Act
        var response = await client.PostAsJsonAsync("/api/pagos", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        content.TryGetProperty("transaccionId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Get_Pagos_RetornaOk_CuandoExiste()
    {
        // Arrange
        var client = _factory.CreateClient();
        var txId = Guid.NewGuid();
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PagosDbContext>();
            db.Transacciones.Add(new Transaccion { Id = txId, OrdenId = Guid.NewGuid(), TarjetaMascara = "****1234" });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/pagos/{txId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Pagos_RetornaNotFound_EnProduccion()
    {
        // Arrange
        var factoryProd = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        var client = factoryProd.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/pagos/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
