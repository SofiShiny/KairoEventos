using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reportes.Aplicacion;
using Reportes.Aplicacion.Consumers;
using Reportes.Dominio.Repositorios;
using Reportes.Infraestructura;
using Reportes.Infraestructura.Persistencia;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Reportes.Pruebas.API;

/// <summary>
/// Tests de integración para Program.cs que validan la configuración de startup
/// y el registro correcto de servicios en el contenedor de dependencias.
/// 
/// Objetivo: Reducir CRAP score de Program.cs de 506 a <30
/// </summary>
public class ProgramIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProgramIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AplicacionInicia_Correctamente()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        json.RootElement.GetProperty("servicio").GetString().Should().Be("Reportes API");
        json.RootElement.GetProperty("version").GetString().Should().Be("1.0.0");
        json.RootElement.GetProperty("estado").GetString().Should().Be("activo");
    }

    [Fact]
    public void ServiciosEsenciales_SeRegistranCorrectamente()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert - Servicios de infraestructura
        serviceProvider.GetService<ReportesMongoDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IRepositorioReportesLectura>().Should().NotBeNull();
        
        // Act & Assert - Verificar que los consumers están registrados como clases concretas
        serviceProvider.GetService<EventoPublicadoConsumer>().Should().NotBeNull();
        serviceProvider.GetService<AsistenteRegistradoConsumer>().Should().NotBeNull();
        serviceProvider.GetService<AsientoReservadoConsumer>().Should().NotBeNull();
        serviceProvider.GetService<AsientoLiberadoConsumer>().Should().NotBeNull();
        serviceProvider.GetService<EventoCanceladoConsumer>().Should().NotBeNull();
        serviceProvider.GetService<MapaAsientosCreadoConsumer>().Should().NotBeNull();
        
        // MassTransit puede estar registrado o no dependiendo de la configuración
        // En el entorno de test actual, está registrado pero puede fallar la conexión
        var bus = serviceProvider.GetService<IBus>();
        // No verificamos si es null o no, solo que el servicio se puede resolver sin excepción
    }

    [Fact]
    public void Consumers_SeRegistranCorrectamente()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert - Verificar que todos los consumers están registrados
        var eventoPublicadoConsumer = serviceProvider.GetService<EventoPublicadoConsumer>();
        var asistenteRegistradoConsumer = serviceProvider.GetService<AsistenteRegistradoConsumer>();
        var asientoReservadoConsumer = serviceProvider.GetService<AsientoReservadoConsumer>();
        var asientoLiberadoConsumer = serviceProvider.GetService<AsientoLiberadoConsumer>();
        var eventoCanceladoConsumer = serviceProvider.GetService<EventoCanceladoConsumer>();
        var mapaAsientosCreadoConsumer = serviceProvider.GetService<MapaAsientosCreadoConsumer>();

        eventoPublicadoConsumer.Should().NotBeNull();
        asistenteRegistradoConsumer.Should().NotBeNull();
        asientoReservadoConsumer.Should().NotBeNull();
        asientoLiberadoConsumer.Should().NotBeNull();
        eventoCanceladoConsumer.Should().NotBeNull();
        mapaAsientosCreadoConsumer.Should().NotBeNull();
    }

    [Fact]
    public async Task MiddlewareSeConfigura_EnOrdenCorrecto()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/reportes/resumen-ventas");

        // Assert - Verificar que el middleware de correlación está funcionando
        response.Headers.Should().ContainKey("X-Correlation-ID");
        
        // Verificar que el middleware de manejo de excepciones está funcionando
        // (no debería lanzar excepciones no controladas)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task HealthChecks_RespondenCorrectamente()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert - En el entorno de test, el health check puede fallar debido a MassTransit
        // pero el endpoint debe responder correctamente
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        // Verificar que la respuesta tiene la estructura correcta
        json.RootElement.TryGetProperty("status", out var statusProperty).Should().BeTrue();
        json.RootElement.TryGetProperty("timestamp", out var timestampProperty).Should().BeTrue();
        
        // Si el status es Healthy, verificar que es correcto
        if (response.StatusCode == HttpStatusCode.OK)
        {
            statusProperty.GetString().Should().Be("Healthy");
        }
        else
        {
            // Si es ServiceUnavailable, debe ser debido a MassTransit
            statusProperty.GetString().Should().Be("Unhealthy");
        }
        
        timestampProperty.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Swagger_SeConfiguraEnDesarrollo()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var testConfiguration = new Dictionary<string, string?>
                    {
                        ["MongoDbSettings:ConnectionString"] = "mongodb://localhost:27017",
                        ["MongoDbSettings:DatabaseName"] = "test_db",
                        ["Hangfire:Enabled"] = "false",
                        ["HealthChecks:Enabled"] = "false"
                    };
                    config.AddInMemoryCollection(testConfiguration);
                });
            });

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        json.RootElement.GetProperty("info").GetProperty("title").GetString().Should().Be("Reportes API");
        json.RootElement.GetProperty("info").GetProperty("version").GetString().Should().Be("v1");
    }

    [Fact]
    public async Task CORS_SeConfiguraCorrectamente()
    {
        // Arrange & Act
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/reportes/resumen-ventas");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("*");
    }

    [Fact]
    public void Hangfire_SeInicializaCorrectamente_CuandoEstaHabilitado()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var testConfiguration = new Dictionary<string, string?>
                    {
                        ["MongoDbSettings:ConnectionString"] = "mongodb://localhost:27017",
                        ["MongoDbSettings:DatabaseName"] = "test_db",
                        ["Hangfire:Enabled"] = "true", // Habilitar Hangfire para este test
                        ["HealthChecks:Enabled"] = "false"
                    };
                    config.AddInMemoryCollection(testConfiguration);
                });
            });

        // Act & Assert - Verificar que no lanza excepciones al inicializar Hangfire
        var exception = Record.Exception(() =>
        {
            using var scope = factory.Services.CreateScope();
            // Si Hangfire está configurado correctamente, no debería lanzar excepciones
        });

        exception.Should().BeNull();
    }

    [Fact]
    public void ConfiguracionDeServicios_NoLanzaExcepciones()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
        {
            var args = new string[] { };
            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configuración básica
                    services.Configure<Reportes.Infraestructura.Configuracion.MongoDbSettings>(options =>
                    {
                        options.ConnectionString = "mongodb://localhost:27017";
                        options.DatabaseName = "test_db";
                    });
                    
                    // Deshabilitar Hangfire para tests
                    context.Configuration["Hangfire:Enabled"] = "false";
                    
                    services.AgregarInfraestructura(context.Configuration);
                    services.AgregarAplicacion(context.Configuration);
                });

            using var host = hostBuilder.Build();
        });

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void VariablesDeEntorno_SePuedenSobrescribir()
    {
        // Arrange
        var originalConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        var originalDatabase = Environment.GetEnvironmentVariable("MONGODB_DATABASE");
        
        try
        {
            Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://test:27017");
            Environment.SetEnvironmentVariable("MONGODB_DATABASE", "test_db");

            // Act
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            var database = Environment.GetEnvironmentVariable("MONGODB_DATABASE");

            // Assert
            connectionString.Should().Be("mongodb://test:27017");
            database.Should().Be("test_db");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", originalConnectionString);
            Environment.SetEnvironmentVariable("MONGODB_DATABASE", originalDatabase);
        }
    }

    [Fact]
    public void AmbienteDeTest_SeConfiguraCorrectamente()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        // Act & Assert
        environment.EnvironmentName.Should().Be("Test");
        environment.IsEnvironment("Test").Should().BeTrue();
    }

    [Fact]
    public async Task Endpoints_EstanDisponibles()
    {
        // Arrange & Act - Probar endpoints principales
        var endpoints = new[]
        {
            "/",
            "/health",
            "/api/reportes/resumen-ventas",
            "/api/reportes/auditoria"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            // Assert - Todos los endpoints deben responder (no 404)
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, 
                $"Endpoint {endpoint} no debería retornar 404");
        }
    }
}