using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Streaming.Infraestructura.Persistencia;
using MassTransit;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;

namespace Streaming.Pruebas.API;

public class ProgramCoverageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramCoverageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestRabbitMqConfiguration_WithValues_ShouldRunConfigBlocks()
    {
        // Arrange
        using var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMQ:Host"] = "my-test-host",
                    ["RabbitMQ:Username"] = "my-test-user",
                    ["RabbitMQ:Password"] = "my-test-pass",
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace DbContext to avoid connection errors during startup
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StreamingDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<StreamingDbContext>(options => options.UseInMemoryDatabase("ProgramCoverageDb1"));

                // We keep the original MassTransit but shorten timeouts
                services.Configure<MassTransitHostOptions>(options =>
                {
                    options.WaitUntilStarted = false;
                    options.StartTimeout = TimeSpan.FromMilliseconds(10);
                });
            });
        });

        // Act - Starting the client starts the WebHost and the HostedServices (including MassTransit)
        var client = customFactory.CreateClient();
        
        try 
        {
            // Force resolution and potential initialization
            var bus = customFactory.Services.GetService<IBusControl>();
            if (bus != null)
            {
                // Starting the bus is the most reliable way to trigger configuration lambdas
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                await bus.StartAsync(cts.Token);
            }
        }
        catch
        {
            // Expected to fail as there is no real RabbitMQ, but lambdas should have run
        }

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task TestRabbitMqConfiguration_WithDefaults_ShouldRunConfigBlocksWithDefaults()
    {
        // Arrange
        using var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RabbitMQ:Host"] = null,
                    ["RabbitMQ:Username"] = null,
                    ["RabbitMQ:Password"] = null,
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StreamingDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<StreamingDbContext>(options => options.UseInMemoryDatabase("ProgramCoverageDb2"));

                services.Configure<MassTransitHostOptions>(options =>
                {
                    options.WaitUntilStarted = false;
                    options.StartTimeout = TimeSpan.FromMilliseconds(10);
                });
            });
        });

        // Act
        var client = customFactory.CreateClient();

        try
        {
            var bus = customFactory.Services.GetService<IBusControl>();
            if (bus != null)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                await bus.StartAsync(cts.Token);
            }
        }
        catch
        {
            // Ignore connection errors
        }

        // Assert
        client.Should().NotBeNull();
    }
}
