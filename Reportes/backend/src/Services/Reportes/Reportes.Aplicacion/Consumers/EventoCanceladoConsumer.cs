using Eventos.Dominio.EventosDeDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class EventoCanceladoConsumer : IConsumer<EventoCanceladoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<EventoCanceladoConsumer> _logger;

    public EventoCanceladoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<EventoCanceladoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventoCanceladoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "Procesando evento EventoCancelado: {EventoId} - {TituloEvento}",
            evento.EventoId,
            evento.TituloEvento);

        try
        {
            // Obtener métricas del evento
            var metricas = await _repositorio.ObtenerMetricasEventoAsync(evento.EventoId);
            
            if (metricas != null)
            {
                // Actualizar estado a Cancelado
                metricas.Estado = "Cancelado";
                metricas.TituloEvento = evento.TituloEvento;
                
                await _repositorio.ActualizarMetricasAsync(metricas);

                _logger.LogInformation(
                    "Estado del evento actualizado a Cancelado: {EventoId}",
                    evento.EventoId);
            }
            else
            {
                // Si no existe, crear métricas con estado Cancelado
                metricas = new MetricasEvento
                {
                    EventoId = evento.EventoId,
                    TituloEvento = evento.TituloEvento,
                    Estado = "Cancelado",
                    FechaCreacion = DateTime.UtcNow
                };

                await _repositorio.ActualizarMetricasAsync(metricas);

                _logger.LogWarning(
                    "Métricas no encontradas para evento {EventoId}, creadas con estado Cancelado",
                    evento.EventoId);
            }

            // Registrar en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Evento",
                EntidadId = evento.EventoId.ToString(),
                Detalles = $"Evento cancelado: {evento.TituloEvento}",
                Exitoso = true
            });

            _logger.LogInformation(
                "Evento EventoCancelado procesado exitosamente: {EventoId}",
                evento.EventoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento EventoCancelado: {EventoId}",
                evento.EventoId);

            // Registrar error en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "ErrorProcesamiento",
                Entidad = "Evento",
                EntidadId = evento.EventoId.ToString(),
                Detalles = $"Error procesando EventoCancelado: {evento.TituloEvento}",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Permitir reintento de MassTransit
        }
    }
}
