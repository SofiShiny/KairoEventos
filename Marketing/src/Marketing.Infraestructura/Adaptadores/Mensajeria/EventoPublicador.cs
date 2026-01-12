using MassTransit;
using Marketing.Aplicacion.Interfaces;

namespace Marketing.Infraestructura.Adaptadores.Mensajeria;

public class EventoPublicador : IEventoPublicador
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventoPublicador(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublicarAsync<T>(T evento) where T : class
    {
        await _publishEndpoint.Publish(evento);
    }
}
