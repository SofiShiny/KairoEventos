using Asientos.Dominio.EventosDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class AsientoAgregadoConsumer : IConsumer<AsientoAgregadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<AsientoAgregadoConsumer> _logger;

    public AsientoAgregadoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<AsientoAgregadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AsientoAgregadoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogDebug(
            "Procesando evento AsientoAgregado: Mapa {MapaId} - Fila {Fila} - Número {Numero}",
            evento.MapaId,
            evento.Fila,
            evento.Numero);

        try
        {
            // Actualizar capacidad total del evento
            var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.MapaId);
            
            if (historial != null)
            {
                historial.CapacidadTotal++;
                historial.AsientosDisponibles++;
                historial.UltimaActualizacion = DateTime.UtcNow;

                await _repositorio.ActualizarAsistenciaAsync(historial);

                _logger.LogDebug(
                    "Capacidad actualizada para evento {EventoId}: {CapacidadTotal} asientos",
                    historial.EventoId,
                    historial.CapacidadTotal);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento AsientoAgregado: Mapa {MapaId}",
                evento.MapaId);
            
            // No lanzar excepción para no bloquear el procesamiento
            // Este evento es informativo y no crítico
        }
    }
}
