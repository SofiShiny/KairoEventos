using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.EventosDeDominio;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Aplicacion.Comandos;

public class CancelarEventoComandoHandler : IRequestHandler<CancelarEventoComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CancelarEventoComandoHandler> _logger;

    public CancelarEventoComandoHandler(
        IRepositorioEvento repositorioEvento,
        IPublishEndpoint publishEndpoint,
        ILogger<CancelarEventoComandoHandler> logger)
    {
        _repositorioEvento = repositorioEvento;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Resultado> Handle(CancelarEventoComando request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando cancelación de evento {EventoId}", request.EventoId);
            
            var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, asNoTracking: false, cancellationToken);
            
            if (evento == null)
            {
                _logger.LogWarning("Evento {EventoId} no encontrado para cancelar", request.EventoId);
                return Resultado.Falla("Evento no encontrado");
            }

            _logger.LogInformation("Cancelando evento {EventoId}, estado actual: {Estado}", request.EventoId, evento.Estado);
            
            evento.Cancelar();
            
            _logger.LogInformation("Evento cancelado, guardando en BD...");
            await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
            _logger.LogInformation("Evento {EventoId} cancelado exitosamente en BD", request.EventoId);
            
            // Publicar evento a RabbitMQ
            _logger.LogInformation("Publicando EventoCancelado a RabbitMQ...");
            await _publishEndpoint.Publish(new EventoCanceladoEventoDominio(
                evento.Id,
                evento.Titulo), cancellationToken);
            _logger.LogInformation("EventoCancelado publicado exitosamente a RabbitMQ");
            
            return Resultado.Exito();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error de operación inválida al cancelar evento {EventoId}", request.EventoId);
            return Resultado.Falla(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cancelar evento {EventoId}. Tipo: {TipoExcepcion}, Mensaje: {Mensaje}", 
                request.EventoId, ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            return Resultado.Falla($"Error inesperado: {ex.Message}");
        }
    }
}
