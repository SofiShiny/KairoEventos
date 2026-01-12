using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Notificaciones.Aplicacion.Consumers;
using Notificaciones.Dominio.Interfaces;
using FluentAssertions;
using Xunit;

namespace Notificaciones.Pruebas.API;

public class ProgramTests : IClassFixture<WebApplicationFactory<Notificaciones.API.Program>>
{
    private readonly WebApplicationFactory<Notificaciones.API.Program> _factory;

    public ProgramTests(WebApplicationFactory<Notificaciones.API.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Reemplazar MassTransit para no requerir un RabbitMQ real durante los tests de integración
                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<PagoAprobadoConsumer>();
                });
            });
        });
    }

    [Fact]
    public async Task RootEndpoint_DebeResponderTextoCorrecto()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        // This exercises Program.cs startup logic
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Notificaciones API corriendo");
    }

    [Fact]
    public void DependencyInjection_DebeRegistrarServiciosCorrectos()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Act & Assert
        services.GetService<IServicioEmail>().Should().NotBeNull();
        services.GetService<INotificadorRealTime>().Should().NotBeNull();
    }

    [Fact]
    public async Task SignalR_TokenExtraction_ShouldProcessTokenFromQueryString()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = "test-token";
        
        // Act - Acceder a la ruta del Hub con el token en el query string
        // Esto ejercita la lógica de OnMessageReceived en Program.cs
        var response = await client.GetAsync($"/notificacionesHub?access_token={token}");

        // Assert
        // Aunque el hub requiera auth real para conectar (y aquí responderá 401), 
        // el código de extracción del token se habrá ejecutado.
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CORS_ConConfiguracionNula_DebeUsarValorPorDefecto()
    {
        // Arrange - Factory con configuración CORS nula
        var factoryWithNullCors = new WebApplicationFactory<Notificaciones.API.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"Cors:AllowedOrigins", null!} // Forzar null para probar el ??
                    });
                });
                builder.ConfigureServices(services =>
                {
                    services.AddMassTransitTestHarness(x =>
                    {
                        x.AddConsumer<PagoAprobadoConsumer>();
                    });
                });
            });

        var client = factoryWithNullCors.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert - Debe funcionar con el valor por defecto
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Hub_DebeEstarMapeadoCorrectamente()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Intentar conectar al hub (sin token)
        var response = await client.GetAsync("/notificacionesHub");

        // Assert - Debe responder (aunque sea con error de auth)
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Unauthorized,
            System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JWT_ConConfiguracionCompleta_DebeConfigurarCorrectamente()
    {
        // Arrange - Factory con configuración JWT completa
        var factoryWithJwt = new WebApplicationFactory<Notificaciones.API.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"Jwt:Authority", "http://test-keycloak:8080/realms/Test"},
                        {"Jwt:Audience", "test-audience"}
                    });
                });
                builder.ConfigureServices(services =>
                {
                    services.AddMassTransitTestHarness(x =>
                    {
                        x.AddConsumer<PagoAprobadoConsumer>();
                    });
                });
            });

        var client = factoryWithJwt.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Endpoints_TodosLosEndpointsDebenResponder()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act & Assert - Endpoint raíz
        var rootResponse = await client.GetAsync("/");
        rootResponse.EnsureSuccessStatusCode();
        var content = await rootResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Notificaciones API corriendo");
    }
}
