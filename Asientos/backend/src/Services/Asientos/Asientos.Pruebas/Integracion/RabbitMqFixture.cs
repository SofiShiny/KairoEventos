using Testcontainers.RabbitMq;
using Xunit;

namespace Asientos.Pruebas.Integracion;

public class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer;

    public RabbitMqFixture()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
    }

    public string ConnectionString => _rabbitMqContainer.GetConnectionString();
    public string Host => _rabbitMqContainer.Hostname;
    public int Port => _rabbitMqContainer.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }
}
