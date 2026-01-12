using Eventos.Dominio.EventosDeDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class EventoPublicadoConsumer : IConsumer<EventoPublicadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<EventoPublicadoConsumer> _logger;

    public EventoPublicadoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<EventoPublicadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventoPublicadoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "Procesando evento EventoPublicado: {EventoId} - {TituloEvento}",
            evento.EventoId,
            evento.TituloEvento);

        try
        {
            // Crear o actualizar métricas del evento
            var metricas = await _repositorio.ObtenerMetricasEventoAsync(evento.EventoId)
                ?? new MetricasEvento
                {
                    EventoId = evento.EventoId,
                    TituloEvento = evento.TituloEvento,
                    FechaInicio = evento.FechaInicio,
                    Estado = "Publicado",
                    FechaCreacion = DateTime.UtcNow
                };

            metricas.TituloEvento = evento.TituloEvento;
            metricas.FechaInicio = evento.FechaInicio;
            metricas.Estado = "Publicado";

            await _repositorio.ActualizarMetricasAsync(metricas);

            // Registrar en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Evento",
                EntidadId = evento.EventoId.ToString(),
                Detalles = $"Evento publicado: {evento.TituloEvento}",
                Exitoso = true
            });

            _logger.LogInformation(
                "Evento EventoPublicado procesado exitosamente: {EventoId}",
                evento.EventoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento EventoPublicado: {EventoId}",
                evento.EventoId);

            // Registrar error en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "ErrorProcesamiento",
                Entidad = "Evento",
                EntidadId = evento.EventoId.ToString(),
                Detalles = $"Error procesando EventoPublicado: {evento.TituloEvento}",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Permitir reintento de MassTransit
        }
    }
}
