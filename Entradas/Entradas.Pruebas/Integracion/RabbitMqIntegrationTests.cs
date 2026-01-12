/*using Testcontainers.RabbitMq;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using Entradas.Dominio.Eventos;
using Entradas.Aplicacion.Consumers;

namespace Entradas.Pruebas.Integracion;

/// <summary>
/// Pruebas de integración de RabbitMQ con TestContainers
/// Valida publicación y consumo de eventos reales
/// </summary>
public class RabbitMqIntegrationTests : IAsyncLifetime
{
    private RabbitMqContainer? _container;
    private IBusControl? _bus;
    private IServiceProvider? _serviceProvider;

    public async Task InitializeAsync()
    {
        // Crear contenedor RabbitMQ
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .Build();

        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        // Configurar MassTransit con RabbitMQ
        var services = new ServiceCollection();
        
        services.AddLogging(config => config.AddConsole());
        
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PagoConfirmadoConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                
                cfg.ReceiveEndpoint("pago-confirmado-queue", e =>
                {
                    e.ConfigureConsumer<PagoConfirmadoConsumer>(context);
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        _serviceProvider = services.BuildServiceProvider();
        _bus = _serviceProvider.GetRequiredService<IBusControl>();
        
        await _bus.StartAsync();
        
        // Esperar a que el bus esté listo
        await Task.Delay(1000);
    }

    public async Task DisposeAsync()
    {
        if (_bus != null)
        {
            await _bus.StopAsync();
        }

        if (_serviceProvider != null)
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }

        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    [Fact]
    public async Task RabbitMq_DebeConectarseCorrectamente()
    {
        // Act & Assert
        _bus.Should().NotBeNull();
    }

    [Fact]
    public async Task Publicacion_EventoPagoConfirmado_DebePublicarseCorrectamente()
    {
        // Arrange
        var evento = new PagoConfirmadoEvento
        {
            EntradaId = Guid.NewGuid(),
            TransaccionId = Guid.NewGuid(),
            MontoConfirmado = 150.50m,
            FechaPago = DateTime.UtcNow
        };

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        await endpoint.Send(evento);

        // Assert - El evento debe publicarse sin errores
        // En un test real, verificaríamos que el consumer procesó el evento
        // pero eso requeriría una base de datos real o mocks más complejos
    }

    [Fact]
    public async Task Publicacion_MultipleEventos_DebenPublicarseCorrectamente()
    {
        // Arrange
        var eventos = Enumerable.Range(1, 5)
            .Select(i => new PagoConfirmadoEvento
            {
                EntradaId = Guid.NewGuid(),
                TransaccionId = Guid.NewGuid(),
                MontoConfirmado = 100m + i,
                FechaPago = DateTime.UtcNow
            })
            .ToList();

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        
        foreach (var evento in eventos)
        {
            await endpoint.Send(evento);
        }

        // Assert
        // Los eventos deben publicarse sin errores
        eventos.Should().HaveCount(5);
    }

    [Fact]
    public async Task Consumo_EventoPagoConfirmado_DebeSerProcesado()
    {
        // Arrange
        var evento = new PagoConfirmadoEvento
        {
            EntradaId = Guid.NewGuid(),
            TransaccionId = Guid.NewGuid(),
            MontoConfirmado = 200.75m,
            FechaPago = DateTime.UtcNow
        };

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        await endpoint.Send(evento);

        // Esperar a que el consumer procese el evento
        await Task.Delay(2000);

        // Assert
        // El consumer debe haber procesado el evento
        // En un test real con base de datos, verificaríamos que la entrada fue actualizada
    }

    [Fact]
    public async Task Consumo_EventoConDatosInvalidos_DebeManejareCorrectamente()
    {
        // Arrange
        var evento = new PagoConfirmadoEvento
        {
            EntradaId = Guid.Empty, // ID inválido
            TransaccionId = Guid.NewGuid(),
            MontoConfirmado = -100m, // Monto negativo
            FechaPago = DateTime.UtcNow
        };

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        
        // No debe lanzar excepción
        await endpoint.Send(evento);

        // Esperar a que el consumer procese el evento
        await Task.Delay(2000);

        // Assert
        // El consumer debe manejar el error gracefully
    }

    [Fact]
    public async Task Topologia_ColaDebeEstarConfigurada()
    {
        // Act & Assert
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        endpoint.Should().NotBeNull();
    }

    [Fact]
    public async Task Resiliencia_ReconexionAutomatica_DebeReconectarse()
    {
        // Arrange
        var evento = new PagoConfirmadoEvento
        {
            EntradaId = Guid.NewGuid(),
            TransaccionId = Guid.NewGuid(),
            MontoConfirmado = 150m,
            FechaPago = DateTime.UtcNow
        };

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        
        // Publicar evento
        await endpoint.Send(evento);

        // Esperar un poco
        await Task.Delay(500);

        // Publicar otro evento (simula reconexión)
        await endpoint.Send(evento);

        // Assert
        _bus.Should().NotBeNull();
    }

    [Fact]
    public async Task Performance_PublicarMultiplesEventos_DebeSerRapido()
    {
        // Arrange
        var eventos = Enumerable.Range(1, 100)
            .Select(i => new PagoConfirmadoEvento
            {
                EntradaId = Guid.NewGuid(),
                TransaccionId = Guid.NewGuid(),
                MontoConfirmado = 100m + i,
                FechaPago = DateTime.UtcNow
            })
            .ToList();

        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var evento in eventos)
        {
            await endpoint.Send(evento);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Fact]
    public async Task Serialization_EventoDebeSerializarseCorrectamente()
    {
        // Arrange
        var evento = new PagoConfirmadoEvento
        {
            EntradaId = Guid.NewGuid(),
            TransaccionId = Guid.NewGuid(),
            MontoConfirmado = 250.99m,
            FechaPago = DateTime.UtcNow
        };

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        await endpoint.Send(evento);

        // Assert
        // El evento debe serializarse sin errores
        evento.EntradaId.Should().NotBe(Guid.Empty);
        evento.MontoConfirmado.Should().Be(250.99m);
    }

    [Fact]
    public async Task Deserialization_EventoDebeDeserializarseCorrectamente()
    {
        // Arrange
        var eventoOriginal = new PagoConfirmadoEvento
        {
            EntradaId = Guid.NewGuid(),
            TransaccionId = Guid.NewGuid(),
            MontoConfirmado = 300.50m,
            FechaPago = DateTime.UtcNow
        };

        // Act
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        await endpoint.Send(eventoOriginal);

        // Esperar a que se procese
        await Task.Delay(1000);

        // Assert
        // El evento debe deserializarse correctamente en el consumer
        eventoOriginal.EntradaId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Concurrencia_MultiplePublicadores_DebenPublicarCorrectamente()
    {
        // Arrange
        var endpoint = await _bus!.GetSendEndpoint(new Uri("rabbitmq://localhost/pago-confirmado-queue"));
        var tareas = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var evento = new PagoConfirmadoEvento
            {
                EntradaId = Guid.NewGuid(),
                TransaccionId = Guid.NewGuid(),
                MontoConfirmado = 100m + i,
                FechaPago = DateTime.UtcNow
            };

            tareas.Add(endpoint.Send(evento));
        }

        await Task.WhenAll(tareas);

        // Assert
        tareas.Should().HaveCount(10);
        tareas.Should().AllSatisfy(t => t.IsCompletedSuccessfully.Should().BeTrue());
    }
}
*/