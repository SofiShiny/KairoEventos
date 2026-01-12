using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.EventosDeDominio;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Aplicacion.Comandos;

public class RegistrarAsistenteComandoHandler : IRequestHandler<RegistrarAsistenteComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RegistrarAsistenteComandoHandler> _logger;

    public RegistrarAsistenteComandoHandler(
        IRepositorioEvento repositorioEvento,
        IPublishEndpoint publishEndpoint,
        ILogger<RegistrarAsistenteComandoHandler> logger)
    {
        _repositorioEvento = repositorioEvento;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Resultado> Handle(RegistrarAsistenteComando request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando registro de asistente para evento {EventoId}", request.EventoId);
        
        // Obtener el evento con tracking habilitado
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, asNoTracking: false, cancellationToken);
        
        if (evento == null)
        {
            _logger.LogWarning("Evento {EventoId} no encontrado", request.EventoId);
            return Resultado.Falla("Evento no encontrado");
        }

        try
        {
            _logger.LogInformation("Registrando asistente {UsuarioId} en evento {EventoId}", request.UsuarioId, request.EventoId);
            _logger.LogDebug("Estado del evento antes de registrar: {Estado}, Asistentes actuales: {Asistentes}", 
                evento.Estado, evento.ConteoAsistentesActual);
            
            // Registrar el asistente (esto agrega a la colección privada)
            evento.RegistrarAsistente(request.UsuarioId, request.NombreUsuario, request.Correo);
            
            _logger.LogDebug("Asistente agregado a la colección, total asistentes: {Total}", evento.ConteoAsistentesActual);
            _logger.LogInformation("Guardando cambios en BD...");
            
            // Guardar cambios
            await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
            _logger.LogInformation("Cambios guardados exitosamente en BD");
            
            // Publicar evento a RabbitMQ
            _logger.LogInformation("Publicando AsistenteRegistrado a RabbitMQ...");
            await _publishEndpoint.Publish(new AsistenteRegistradoEventoDominio(
                request.EventoId,
                request.UsuarioId,
                request.NombreUsuario), cancellationToken);
            _logger.LogInformation("AsistenteRegistrado publicado exitosamente a RabbitMQ");
            
            return Resultado.Exito();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error de operación inválida al registrar asistente en evento {EventoId}", request.EventoId);
            return Resultado.Falla(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar asistente en evento {EventoId}. Tipo: {TipoExcepcion}, Mensaje: {Mensaje}", 
                request.EventoId, ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            return Resultado.Falla($"Error inesperado: {ex.Message}");
        }
    }
}
