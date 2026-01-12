using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using MediatR;
using FluentValidation;
using Entradas.Dominio.Interfaces;
using Entradas.Infraestructura.Persistencia;
using Entradas.Infraestructura.Servicios;
using Entradas.Aplicacion.Handlers;
using Entradas.API.HealthChecks;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using Moq;
using MassTransit;

namespace Entradas.Pruebas.API;

/// <summary>
/// Pruebas unitarias para la configuración de Program.cs
/// Valida que todos los servicios estén correctamente registrados usando mocks
/// </summary>
public class ProgramIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceProvider _serviceProvider;

    public ProgramIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Configurar factory con mocks para evitar conexiones reales
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Reemplazar DbContext con InMemory para evitar PostgreSQL real
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<EntradasDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<EntradasDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // Mock de servicios externos para evitar HTTP calls
                var mockVerificadorEventos = new Mock<IVerificadorEventos>();
                mockVerificadorEventos
                    .Setup(x => x.EventoExisteYDisponibleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

                var mockVerificadorAsientos = new Mock<IVerificadorAsientos>();
                mockVerificadorAsientos
                    .Setup(x => x.AsientoDisponibleAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

                // Reemplazar servicios reales con mocks
                services.AddScoped(_ => mockVerificadorEventos.Object);
                services.AddScoped(_ => mockVerificadorAsientos.Object);

                // Mock de MassTransit para evitar RabbitMQ real
                services.AddMassTransitTestHarness();
            });
        });
        
        _serviceProvider = _factory.Services;
    }

    #region Service Registration Tests

    [Fact]
    public void Program_DebeRegistrarServiciosBasicos_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert - Servicios básicos de ASP.NET Core
        services.GetService<IConfiguration>().Should().NotBeNull();
        services.GetService<ILogger<Program>>().Should().NotBeNull();
        services.GetService<IWebHostEnvironment>().Should().NotBeNull();
    }

    [Fact]
    public void Program_DebeRegistrarMediatR_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var mediator = services.GetService<IMediator>();
        mediator.Should().NotBeNull();

        // Verificar que MediatR esté configurado correctamente usando ISender
        var sender = services.GetService<ISender>();
        sender.Should().NotBeNull("porque ISender de MediatR debe estar registrado");

        // Verificar que MediatR esté configurado correctamente usando IPublisher
        var publisher = services.GetService<IPublisher>();
        publisher.Should().NotBeNull("porque IPublisher de MediatR debe estar registrado");
    }

    [Fact]
    public void Program_DebeRegistrarFluentValidation_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert - Verificar que el sistema de validación esté configurado
        var validatorFactory = services.GetService<IValidatorFactory>();
        validatorFactory.Should().NotBeNull("porque FluentValidation debe estar configurado");
    }

    [Fact]
    public void Program_DebeRegistrarEntityFramework_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var dbContext = services.GetService<EntradasDbContext>();
        dbContext.Should().NotBeNull();
        
        // Verificar que la configuración de la base de datos esté correcta
        var database = dbContext!.Database;
        database.Should().NotBeNull();
    }

    [Fact]
    public void Program_DebeRegistrarRepositorios_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var repositorioEntradas = services.GetService<IRepositorioEntradas>();
        repositorioEntradas.Should().NotBeNull();

        var unitOfWork = services.GetService<IUnitOfWork>();
        unitOfWork.Should().NotBeNull();
    }

    [Fact]
    public void Program_DebeRegistrarServiciosDeDominio_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var generadorCodigoQr = services.GetService<IGeneradorCodigoQr>();
        generadorCodigoQr.Should().NotBeNull();

        var verificadorEventos = services.GetService<IVerificadorEventos>();
        verificadorEventos.Should().NotBeNull();

        var verificadorAsientos = services.GetService<IVerificadorAsientos>();
        verificadorAsientos.Should().NotBeNull();
    }

    [Fact]
    public void Program_DebeRegistrarMetricas_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var meter = services.GetService<Meter>();
        meter.Should().NotBeNull();
        meter!.Name.Should().Be("Entradas.API");

        var activitySource = services.GetService<ActivitySource>();
        activitySource.Should().NotBeNull();
        activitySource!.Name.Should().Be("Entradas.API");

        var entradasMetrics = services.GetService<IEntradasMetrics>();
        entradasMetrics.Should().NotBeNull();
    }

    [Fact]
    public void Program_DebeRegistrarHealthChecks_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var healthCheckService = services.GetService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
        healthCheckService.Should().NotBeNull();

        // Verificar que las opciones de health checks estén configuradas
        var healthCheckOptions = services.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckServiceOptions>>();
        healthCheckOptions.Should().NotBeNull("porque las opciones de health checks deben estar configuradas");

        // Verificar que hay health checks registrados
        var healthCheckRegistrations = healthCheckOptions.Value.Registrations;
        healthCheckRegistrations.Should().NotBeEmpty("porque debe haber al menos un health check registrado");
        
        // Verificar que el health check personalizado esté registrado
        healthCheckRegistrations.Should().ContainSingle(r => r.Name == "entrada-service", 
            "porque EntradaServiceHealthCheck debe estar registrado");
    }

    #endregion

    #region HTTP Pipeline Tests

    [Fact]
    public async Task Program_DebeConfigurarHealthChecks_Correctamente()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert - Solo verificar que el endpoint responde
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Program_DebeConfigurarMiddlewareDeExcepciones_Correctamente()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Intentar acceder a un endpoint que no existe
        var response = await client.GetAsync("/api/entradas/endpoint-inexistente");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Program_DebeCargarConfiguracion_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Assert
        configuration.Should().NotBeNull();
        
        // Verificar que las secciones de configuración estén disponibles
        var connectionStrings = configuration.GetSection("ConnectionStrings");
        connectionStrings.Should().NotBeNull();

        var logging = configuration.GetSection("Logging");
        logging.Should().NotBeNull();
    }

    [Fact]
    public void Program_DebeConfigurarLogging_Correctamente()
    {
        // Arrange & Act
        using var scope = _serviceProvider.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        // Assert
        loggerFactory.Should().NotBeNull();
        
        var logger = loggerFactory.CreateLogger<ProgramIntegrationTests>();
        logger.Should().NotBeNull();
    }

    #endregion

    #region Service Lifetime Tests

    [Fact]
    public void Program_DebeRegistrarServiciosConLifetimeCorrect_Correctamente()
    {
        // Arrange & Act
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        // Assert - Servicios Singleton
        var meter1 = scope1.ServiceProvider.GetService<Meter>();
        var meter2 = scope2.ServiceProvider.GetService<Meter>();
        meter1.Should().BeSameAs(meter2);

        // Assert - Servicios Scoped
        var dbContext1 = scope1.ServiceProvider.GetService<EntradasDbContext>();
        var dbContext2 = scope2.ServiceProvider.GetService<EntradasDbContext>();
        dbContext1.Should().NotBeSameAs(dbContext2);

        var repositorio1 = scope1.ServiceProvider.GetService<IRepositorioEntradas>();
        var repositorio2 = scope2.ServiceProvider.GetService<IRepositorioEntradas>();
        repositorio1.Should().NotBeSameAs(repositorio2);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Program_DebeManejearErrores500_Correctamente()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Hacer una petición que podría causar un error interno
        var response = await client.PostAsync("/api/entradas", 
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        // El middleware de excepciones debe manejar cualquier error interno
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Swagger Configuration Tests

    [Fact]
    public async Task Program_DebeConfigurarSwagger_EnDesarrollo()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        // En desarrollo, Swagger debe estar disponible
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}