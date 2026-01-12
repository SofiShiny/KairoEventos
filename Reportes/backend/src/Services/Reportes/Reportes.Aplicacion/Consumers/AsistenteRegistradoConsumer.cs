using Eventos.Dominio.EventosDeDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class AsistenteRegistradoConsumer : IConsumer<AsistenteRegistradoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<AsistenteRegistradoConsumer> _logger;

    public AsistenteRegistradoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<AsistenteRegistradoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AsistenteRegistradoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "Procesando evento AsistenteRegistrado: Usuario {UsuarioId} - Evento {EventoId}",
            evento.UsuarioId,
            evento.EventoId);

        try
        {
            // Obtener o crear historial de asistencia
            var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.EventoId)
                ?? new HistorialAsistencia
                {
                    EventoId = evento.EventoId,
                    Asistentes = new List<RegistroAsistente>()
                };

            // Verificar si el asistente ya está registrado
            if (!historial.Asistentes.Any(a => a.UsuarioId == evento.UsuarioId))
            {
                // Incrementar contador de asistentes
                historial.TotalAsistentesRegistrados++;

                // Agregar asistente a la lista
                historial.Asistentes.Add(new RegistroAsistente
                {
                    UsuarioId = evento.UsuarioId,
                    NombreUsuario = evento.NombreUsuario,
                    FechaRegistro = DateTime.UtcNow
                });

                historial.UltimaActualizacion = DateTime.UtcNow;

                await _repositorio.ActualizarAsistenciaAsync(historial);

                // Actualizar métricas del evento
                var metricas = await _repositorio.ObtenerMetricasEventoAsync(evento.EventoId);
                if (metricas != null)
                {
                    metricas.TotalAsistentes++;
                    await _repositorio.ActualizarMetricasAsync(metricas);
                }

                _logger.LogInformation(
                    "Asistente registrado exitosamente: {UsuarioId} para evento {EventoId}",
                    evento.UsuarioId,
                    evento.EventoId);
            }
            else
            {
                _logger.LogWarning(
                    "Asistente {UsuarioId} ya está registrado para evento {EventoId}",
                    evento.UsuarioId,
                    evento.EventoId);
            }

            // Registrar en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Asistente",
                EntidadId = evento.UsuarioId,
                Detalles = $"Asistente {evento.NombreUsuario} registrado para evento {evento.EventoId}",
                Exitoso = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento AsistenteRegistrado: Usuario {UsuarioId} - Evento {EventoId}",
                evento.UsuarioId,
                evento.EventoId);

            // Registrar error en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "ErrorProcesamiento",
                Entidad = "Asistente",
                EntidadId = evento.UsuarioId,
                Detalles = $"Error registrando asistente {evento.NombreUsuario}",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Permitir reintento de MassTransit
        }
    }
}
