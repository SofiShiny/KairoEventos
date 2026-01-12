using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.EventosDeDominio;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Aplicacion.Comandos;

public class PublicarEventoComandoHandler : IRequestHandler<PublicarEventoComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PublicarEventoComandoHandler> _logger;

    public PublicarEventoComandoHandler(
        IRepositorioEvento repositorioEvento,
        IPublishEndpoint publishEndpoint,
        ILogger<PublicarEventoComandoHandler> logger)
    {
        _repositorioEvento = repositorioEvento;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Resultado> Handle(PublicarEventoComando request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando publicación de evento {EventoId}", request.EventoId);
        
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, asNoTracking: false, cancellationToken);
        
        if (evento == null)
        {
            _logger.LogWarning("Evento {EventoId} no encontrado", request.EventoId);
            return Resultado.Falla("Evento no encontrado");
        }

        try
        {
            _logger.LogInformation("Evento {EventoId} encontrado, estado actual: {Estado}", evento.Id, evento.Estado);
            
            evento.Publicar();
            _logger.LogInformation("Evento {EventoId} marcado como publicado, guardando en BD...", evento.Id);
            
            await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
            _logger.LogInformation("Evento {EventoId} guardado exitosamente en BD", evento.Id);
            
            // Verificar que el evento se guardó correctamente
            var eventoVerificacion = await _repositorioEvento.ObtenerPorIdAsync(evento.Id, asNoTracking: true, cancellationToken);
            if (eventoVerificacion == null)
            {
                _logger.LogError("ERROR CRÍTICO: Evento {EventoId} no se encuentra después de guardar", evento.Id);
                return Resultado.Falla("Error al guardar el evento");
            }
            _logger.LogInformation("Verificación OK: Evento {EventoId} existe con estado {Estado}", eventoVerificacion.Id, eventoVerificacion.Estado);
            
            // Publicar evento a RabbitMQ
            _logger.LogInformation("Publicando evento {EventoId} a RabbitMQ...", evento.Id);
            await _publishEndpoint.Publish(new EventoPublicadoEventoDominio(
                evento.Id,
                evento.Titulo,
                evento.FechaInicio), cancellationToken);
            _logger.LogInformation("Evento {EventoId} publicado exitosamente a RabbitMQ", evento.Id);
            
            return Resultado.Exito();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error de operación inválida al publicar evento {EventoId}", request.EventoId);
            return Resultado.Falla(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al publicar evento {EventoId}", request.EventoId);
            return Resultado.Falla($"Error inesperado: {ex.Message}");
        }
    }
}
