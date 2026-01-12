using Asientos.Dominio.EventosDominio;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class MapaAsientosCreadoConsumer : IConsumer<MapaAsientosCreadoEventoDominio>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<MapaAsientosCreadoConsumer> _logger;

    public MapaAsientosCreadoConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<MapaAsientosCreadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MapaAsientosCreadoEventoDominio> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation(
            "Procesando evento MapaAsientosCreado: Mapa {MapaId} - Evento {EventoId}",
            evento.MapaId,
            evento.EventoId);

        try
        {
            // Crear o actualizar historial de asistencia
            var historial = await _repositorio.ObtenerAsistenciaEventoAsync(evento.EventoId)
                ?? new HistorialAsistencia
                {
                    EventoId = evento.EventoId,
                    Asistentes = new List<RegistroAsistente>()
                };

            // Inicializar valores si es nuevo
            if (string.IsNullOrEmpty(historial.Id) || historial.Id == MongoDB.Bson.ObjectId.GenerateNewId().ToString())
            {
                historial.CapacidadTotal = 0; // Se actualizará cuando se agreguen asientos
                historial.AsientosReservados = 0;
                historial.AsientosDisponibles = 0;
                historial.PorcentajeOcupacion = 0;
            }

            historial.UltimaActualizacion = DateTime.UtcNow;

            await _repositorio.ActualizarAsistenciaAsync(historial);

            // Registrar en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "EventoConsumido",
                Entidad = "MapaAsientos",
                EntidadId = evento.MapaId.ToString(),
                Detalles = $"Mapa de asientos creado para evento {evento.EventoId}",
                Exitoso = true
            });

            _logger.LogInformation(
                "Evento MapaAsientosCreado procesado exitosamente: Mapa {MapaId}",
                evento.MapaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error procesando evento MapaAsientosCreado: Mapa {MapaId}",
                evento.MapaId);

            // Registrar error en auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "ErrorProcesamiento",
                Entidad = "MapaAsientos",
                EntidadId = evento.MapaId.ToString(),
                Detalles = $"Error creando mapa de asientos para evento {evento.EventoId}",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Permitir reintento de MassTransit
        }
    }
}
