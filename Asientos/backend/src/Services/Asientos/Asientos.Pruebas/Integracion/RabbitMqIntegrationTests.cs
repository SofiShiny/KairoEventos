using Xunit;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Asientos.Infraestructura.Persistencia;
using Asientos.Infraestructura.Repositorios;
using Asientos.Dominio.Repositorios;
using Asientos.Aplicacion.Handlers;
using Asientos.Aplicacion.Comandos;
using Asientos.Dominio.EventosDominio;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Asientos.Pruebas.Integracion;

public class RabbitMqIntegrationTests : IClassFixture<RabbitMqFixture>, IAsyncLifetime
{
    private readonly RabbitMqFixture _rabbitMqFixture;
    private ServiceProvider? _serviceProvider;
    private IBusControl? _busControl;
    private readonly ConcurrentBag<object> _receivedEvents = new();

    public RabbitMqIntegrationTests(RabbitMqFixture rabbitMqFixture)
    {
        _rabbitMqFixture = rabbitMqFixture;
    }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Configure DbContext with InMemory database
        services.AddDbContext<AsientosDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Register repository
        services.AddScoped<IRepositorioMapaAsientos, MapaAsientosRepository>();

        // Configure MassTransit with RabbitMQ
        services.AddMassTransit(x =>
        {
            // Register consumers to capture events
            x.AddConsumer<MapaAsientosCreadoConsumer>();
            x.AddConsumer<AsientoAgregadoConsumer>();
            x.AddConsumer<AsientoReservadoConsumer>();
            x.AddConsumer<AsientoLiberadoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_rabbitMqFixture.ConnectionString);

                cfg.ConfigureEndpoints(context);
            });
        });

        // Register handlers and mocks
        services.AddScoped<CrearMapaAsientosComandoHandler>();
        services.AddScoped<AgregarAsientoComandoHandler>();
        services.AddScoped<AgregarCategoriaComandoHandler>();
        services.AddScoped<ReservarAsientoComandoHandler>();
        services.AddScoped<LiberarAsientoComandoHandler>();

        var mockHub = new Mock<IHubContext<Asientos.Aplicacion.Hubs.AsientosHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockHub.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        services.AddSingleton(mockHub.Object);

        // Register event bag for consumers
        services.AddSingleton(_receivedEvents);

        _serviceProvider = services.BuildServiceProvider();
        _busControl = _serviceProvider.GetRequiredService<IBusControl>();
        await _busControl.StartAsync();

        // Wait for bus to be ready
        await Task.Delay(2000);
    }

    public async Task DisposeAsync()
    {
        if (_busControl != null)
        {
            await _busControl.StopAsync();
        }
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task CrearMapa_Should_PublishEventToRabbitMQ()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CrearMapaAsientosComandoHandler>();
        var eventoId = Guid.NewGuid();
        var comando = new CrearMapaAsientosComando(eventoId);

        // Act
        var mapaId = await handler.Handle(comando, CancellationToken.None);

        // Wait for event to be processed
        await Task.Delay(1000);

        // Assert
        mapaId.Should().NotBeEmpty();
        
        var receivedEvent = _receivedEvents
            .OfType<MapaAsientosCreadoEventoDominio>()
            .FirstOrDefault(e => e.MapaId == mapaId);

        receivedEvent.Should().NotBeNull();
        receivedEvent!.MapaId.Should().Be(mapaId);
        receivedEvent.EventoId.Should().Be(eventoId);
        receivedEvent.IdAgregado.Should().Be(mapaId);
    }

    [Fact]
    public async Task AgregarAsiento_Should_PublishEventToRabbitMQ()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var crearHandler = scope.ServiceProvider.GetRequiredService<CrearMapaAsientosComandoHandler>();
        var agregarHandler = scope.ServiceProvider.GetRequiredService<AgregarAsientoComandoHandler>();
        
        var eventoId = Guid.NewGuid();
        var mapaId = await crearHandler.Handle(new CrearMapaAsientosComando(eventoId), CancellationToken.None);
        
        // Add category first
        var categoriaHandler = scope.ServiceProvider.GetRequiredService<AgregarCategoriaComandoHandler>();
        await categoriaHandler.Handle(
            new AgregarCategoriaComando(mapaId, "VIP", 100m, true), 
            CancellationToken.None);

        var comando = new AgregarAsientoComando(mapaId, 1, 1, "VIP");

        // Act
        var asientoId = await agregarHandler.Handle(comando, CancellationToken.None);

        // Wait for event to be processed
        await Task.Delay(1000);

        // Assert
        asientoId.Should().NotBeEmpty();
        
        var receivedEvent = _receivedEvents
            .OfType<AsientoAgregadoEventoDominio>()
            .FirstOrDefault(e => e.MapaId == mapaId && e.Fila == 1 && e.Numero == 1);

        receivedEvent.Should().NotBeNull();
        receivedEvent!.MapaId.Should().Be(mapaId);
        receivedEvent.Fila.Should().Be(1);
        receivedEvent.Numero.Should().Be(1);
        receivedEvent.Categoria.Should().Be("VIP");
        receivedEvent.IdAgregado.Should().Be(mapaId);
    }

    [Fact]
    public async Task ReservarAsiento_Should_PublishEventToRabbitMQ()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var crearHandler = scope.ServiceProvider.GetRequiredService<CrearMapaAsientosComandoHandler>();
        var categoriaHandler = scope.ServiceProvider.GetRequiredService<AgregarCategoriaComandoHandler>();
        var agregarHandler = scope.ServiceProvider.GetRequiredService<AgregarAsientoComandoHandler>();
        var reservarHandler = scope.ServiceProvider.GetRequiredService<ReservarAsientoComandoHandler>();
        
        var eventoId = Guid.NewGuid();
        var mapaId = await crearHandler.Handle(new CrearMapaAsientosComando(eventoId), CancellationToken.None);
        await categoriaHandler.Handle(new AgregarCategoriaComando(mapaId, "General", 50m, false), CancellationToken.None);
        var asientoId = await agregarHandler.Handle(new AgregarAsientoComando(mapaId, 2, 5, "General"), CancellationToken.None);

        var comando = new ReservarAsientoComando(mapaId, asientoId, Guid.NewGuid());

        // Act
        await reservarHandler.Handle(comando, CancellationToken.None);

        // Wait for event to be processed
        await Task.Delay(1000);

        // Assert
        var receivedEvent = _receivedEvents
            .OfType<AsientoReservadoEventoDominio>()
            .FirstOrDefault(e => e.MapaId == mapaId && e.Fila == 2 && e.Numero == 5);

        receivedEvent.Should().NotBeNull();
        receivedEvent!.MapaId.Should().Be(mapaId);
        receivedEvent.Fila.Should().Be(2);
        receivedEvent.Numero.Should().Be(5);
        receivedEvent.IdAgregado.Should().Be(mapaId);
    }

    [Fact]
    public async Task LiberarAsiento_Should_PublishEventToRabbitMQ()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var crearHandler = scope.ServiceProvider.GetRequiredService<CrearMapaAsientosComandoHandler>();
        var categoriaHandler = scope.ServiceProvider.GetRequiredService<AgregarCategoriaComandoHandler>();
        var agregarHandler = scope.ServiceProvider.GetRequiredService<AgregarAsientoComandoHandler>();
        var reservarHandler = scope.ServiceProvider.GetRequiredService<ReservarAsientoComandoHandler>();
        var liberarHandler = scope.ServiceProvider.GetRequiredService<LiberarAsientoComandoHandler>();
        
        var eventoId = Guid.NewGuid();
        var mapaId = await crearHandler.Handle(new CrearMapaAsientosComando(eventoId), CancellationToken.None);
        await categoriaHandler.Handle(new AgregarCategoriaComando(mapaId, "General", 50m, false), CancellationToken.None);
        var asientoId = await agregarHandler.Handle(new AgregarAsientoComando(mapaId, 3, 10, "General"), CancellationToken.None);
        await reservarHandler.Handle(new ReservarAsientoComando(mapaId, asientoId, Guid.NewGuid()), CancellationToken.None);

        var comando = new LiberarAsientoComando(mapaId, asientoId);

        // Act
        await liberarHandler.Handle(comando, CancellationToken.None);

        // Wait for event to be processed
        await Task.Delay(1000);

        // Assert
        var receivedEvent = _receivedEvents
            .OfType<AsientoLiberadoEventoDominio>()
            .FirstOrDefault(e => e.MapaId == mapaId && e.Fila == 3 && e.Numero == 10);

        receivedEvent.Should().NotBeNull();
        receivedEvent!.MapaId.Should().Be(mapaId);
        receivedEvent.Fila.Should().Be(3);
        receivedEvent.Numero.Should().Be(10);
        receivedEvent.IdAgregado.Should().Be(mapaId);
    }

    [Fact]
    public async Task MultipleEvents_Should_AllBePublishedToRabbitMQ()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var crearHandler = scope.ServiceProvider.GetRequiredService<CrearMapaAsientosComandoHandler>();
        var categoriaHandler = scope.ServiceProvider.GetRequiredService<AgregarCategoriaComandoHandler>();
        var agregarHandler = scope.ServiceProvider.GetRequiredService<AgregarAsientoComandoHandler>();
        
        var eventoId = Guid.NewGuid();

        // Act - Create multiple events
        var mapaId = await crearHandler.Handle(new CrearMapaAsientosComando(eventoId), CancellationToken.None);
        await categoriaHandler.Handle(new AgregarCategoriaComando(mapaId, "VIP", 100m, true), CancellationToken.None);
        await agregarHandler.Handle(new AgregarAsientoComando(mapaId, 1, 1, "VIP"), CancellationToken.None);
        await agregarHandler.Handle(new AgregarAsientoComando(mapaId, 1, 2, "VIP"), CancellationToken.None);

        // Wait for all events to be processed
        await Task.Delay(2000);

        // Assert - All events should be received
        _receivedEvents.OfType<MapaAsientosCreadoEventoDominio>()
            .Count(e => e.MapaId == mapaId).Should().BeGreaterOrEqualTo(1);
        
        _receivedEvents.OfType<AsientoAgregadoEventoDominio>()
            .Count(e => e.MapaId == mapaId).Should().BeGreaterOrEqualTo(2);
    }
}

// Consumer implementations to capture events
public class MapaAsientosCreadoConsumer : IConsumer<MapaAsientosCreadoEventoDominio>
{
    private readonly ConcurrentBag<object> _receivedEvents;

    public MapaAsientosCreadoConsumer(ConcurrentBag<object> receivedEvents)
    {
        _receivedEvents = receivedEvents;
    }

    public Task Consume(ConsumeContext<MapaAsientosCreadoEventoDominio> context)
    {
        _receivedEvents.Add(context.Message);
        return Task.CompletedTask;
    }
}

public class AsientoAgregadoConsumer : IConsumer<AsientoAgregadoEventoDominio>
{
    private readonly ConcurrentBag<object> _receivedEvents;

    public AsientoAgregadoConsumer(ConcurrentBag<object> receivedEvents)
    {
        _receivedEvents = receivedEvents;
    }

    public Task Consume(ConsumeContext<AsientoAgregadoEventoDominio> context)
    {
        _receivedEvents.Add(context.Message);
        return Task.CompletedTask;
    }
}

public class AsientoReservadoConsumer : IConsumer<AsientoReservadoEventoDominio>
{
    private readonly ConcurrentBag<object> _receivedEvents;

    public AsientoReservadoConsumer(ConcurrentBag<object> receivedEvents)
    {
        _receivedEvents = receivedEvents;
    }

    public Task Consume(ConsumeContext<AsientoReservadoEventoDominio> context)
    {
        _receivedEvents.Add(context.Message);
        return Task.CompletedTask;
    }
}

public class AsientoLiberadoConsumer : IConsumer<AsientoLiberadoEventoDominio>
{
    private readonly ConcurrentBag<object> _receivedEvents;

    public AsientoLiberadoConsumer(ConcurrentBag<object> receivedEvents)
    {
        _receivedEvents = receivedEvents;
    }

    public Task Consume(ConsumeContext<AsientoLiberadoEventoDominio> context)
    {
        _receivedEvents.Add(context.Message);
        return Task.CompletedTask;
    }
}

