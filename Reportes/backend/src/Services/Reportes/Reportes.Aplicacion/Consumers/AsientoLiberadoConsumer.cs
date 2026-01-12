using Asientos.Dominio.EventosDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class AsientoLiberadoConsumer : IConsumer<AsientoLiberadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<AsientoLiberadoConsumer> _logger;

    public AsientoLiberadoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<AsientoLiberadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AsientoLiberadoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "Procesando evento AsientoLiberado: Mapa {MapaId} - Fila {Fila} - Número {Numero}",
            evento.MapaId,
            evento.Fila,
            evento.Numero);

        try
        {
            // Actualizar historial de asistencia
            var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.MapaId);
            
            if (historial != null)
            {
                // Decrementar asientos reservados
                if (historial.AsientosReservados > 0)
                {
                    historial.AsientosReservados--;
                }

                // Incrementar asientos disponibles
                historial.AsientosDisponibles++;

                // Recalcular porcentaje de ocupación
                if (historial.CapacidadTotal > 0)
                {
                    historial.PorcentajeOcupacion = 
                        (double)historial.AsientosReservados / historial.CapacidadTotal * 100;
                }

                historial.UltimaActualizacion = DateTime.UtcNow;

                await _repositorio.ActualizarAsistenciaAsync(historial);

                _logger.LogInformation(
                    "Historial de asistencia actualizado: Evento {EventoId} - Ocupación {Porcentaje}%",
                    historial.EventoId,
                    historial.PorcentajeOcupacion);
            }
            else
            {
                _logger.LogWarning(
                    "No se encontró historial de asistencia para MapaId {MapaId}",
                    evento.MapaId);
            }

            // Registrar en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Asiento",
                EntidadId = $"{evento.MapaId}_{evento.Fila}_{evento.Numero}",
                Detalles = $"Asiento liberado: Fila {evento.Fila}, Número {evento.Numero}",
                Exitoso = true
            });

            _logger.LogInformation(
                "Evento AsientoLiberado procesado exitosamente: Mapa {MapaId}",
                evento.MapaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento AsientoLiberado: Mapa {MapaId} - Fila {Fila} - Número {Numero}",
                evento.MapaId,
                evento.Fila,
                evento.Numero);

            // Registrar error en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "ErrorProcesamiento",
                Entidad = "Asiento",
                EntidadId = $"{evento.MapaId}_{evento.Fila}_{evento.Numero}",
                Detalles = $"Error liberando asiento: Fila {evento.Fila}, Número {evento.Numero}",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Permitir reintento de MassTransit
        }
    }
}
