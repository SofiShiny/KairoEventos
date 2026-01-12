using Asientos.Dominio.EventosDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class AsientoReservadoConsumer : IConsumer<AsientoReservadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<AsientoReservadoConsumer> _logger;

    public AsientoReservadoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<AsientoReservadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AsientoReservadoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "Procesando evento AsientoReservado: Mapa {MapaId} - Fila {Fila} - Número {Numero}",
            evento.MapaId,
            evento.Fila,
            evento.Numero);

        try
        {
            var fecha = DateTime.UtcNow.Date;

            // Actualizar reporte de ventas diarias
            var reporte = await _repositorio.ObtenerVentasDiariasAsync(fecha)
                ?? new ReporteVentasDiarias
                {
                    Fecha = fecha,
                    ReservasPorCategoria = new Dictionary<string, int>()
                };

            reporte.CantidadReservas++;
            reporte.UltimaActualizacion = DateTime.UtcNow;

            await _repositorio.ActualizarVentasDiariasAsync(reporte);

            // Actualizar historial de asistencia
            // Nota: MapaId corresponde al EventoId en el contexto de asientos
            var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.MapaId);
            
            if (historial != null)
            {
                historial.AsientosReservados++;
                
                if (historial.AsientosDisponibles > 0)
                {
                    historial.AsientosDisponibles--;
                }

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

            // Actualizar métricas del evento
            var metricas = await _repositorio.ObtenerMetricasEventoAsync(evento.MapaId);
            if (metricas != null)
            {
                metricas.TotalReservas++;
                await _repositorio.ActualizarMetricasAsync(metricas);
            }

            // Registrar en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "Asiento",
                EntidadId = $"{evento.MapaId}_{evento.Fila}_{evento.Numero}",
                Detalles = $"Asiento reservado: Fila {evento.Fila}, Número {evento.Numero}",
                Exitoso = true
            });

            _logger.LogInformation(
                "Evento AsientoReservado procesado exitosamente: Mapa {MapaId}",
                evento.MapaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento AsientoReservado: Mapa {MapaId} - Fila {Fila} - Número {Numero}",
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
                Detalles = $"Error reservando asiento: Fila {evento.Fila}, Número {evento.Numero}",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Permitir reintento de MassTransit
        }
    }
}
