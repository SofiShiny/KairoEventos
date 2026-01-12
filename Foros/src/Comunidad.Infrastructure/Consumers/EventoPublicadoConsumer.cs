using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using Eventos.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Comunidad.Infrastructure.Consumers;

public class EventoPublicadoConsumer : IConsumer<EventoPublicadoEventoDominio>
{
    private readonly IForoRepository _foroRepository;
    private readonly ILogger<EventoPublicadoConsumer> _logger;

    public EventoPublicadoConsumer(
        IForoRepository foroRepository,
        ILogger<EventoPublicadoConsumer> logger)
    {
        _foroRepository = foroRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventoPublicadoEventoDominio> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "Recibido evento EventoPublicado: EventoId={EventoId}, Titulo={Titulo}",
            evento.EventoId,
            evento.TituloEvento);

        try
        {
            // Verificar si ya existe un foro para este evento
            var existe = await _foroRepository.ExistePorEventoIdAsync(evento.EventoId);
            if (existe)
            {
                _logger.LogWarning(
                    "Ya existe un foro para el evento {EventoId}. Ignorando mensaje duplicado.",
                    evento.EventoId);
                return;
            }

            // Crear foro automáticamente
            var foro = Foro.Crear(evento.EventoId, evento.TituloEvento);
            await _foroRepository.CrearAsync(foro);

            _logger.LogInformation(
                "Foro creado exitosamente: ForoId={ForoId}, EventoId={EventoId}",
                foro.Id,
                evento.EventoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al crear foro para evento {EventoId}",
                evento.EventoId);
            throw;
        }
    }
}
