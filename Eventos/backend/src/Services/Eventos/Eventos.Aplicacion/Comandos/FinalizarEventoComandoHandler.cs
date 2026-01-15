using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Aplicacion.Comandos;

public class FinalizarEventoComandoHandler : IRequestHandler<FinalizarEventoComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly ILogger<FinalizarEventoComandoHandler> _logger;

    public FinalizarEventoComandoHandler(
        IRepositorioEvento repositorioEvento,
        ILogger<FinalizarEventoComandoHandler> logger)
    {
        _repositorioEvento = repositorioEvento;
        _logger = logger;
    }

    public async Task<Resultado> Handle(FinalizarEventoComando request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando evento {EventoId}", request.EventoId);
        
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, asNoTracking: false, cancellationToken);
        
        if (evento == null)
        {
            _logger.LogWarning("Evento {EventoId} no encontrado", request.EventoId);
            return Resultado.Falla("Evento no encontrado");
        }

        try
        {
            evento.Finalizar();
            await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
            
            _logger.LogInformation("Evento {EventoId} finalizado exitosamente", request.EventoId);
            return Resultado.Exito();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error al finalizar evento {EventoId}", request.EventoId);
            return Resultado.Falla(ex.Message);
        }
    }
}
