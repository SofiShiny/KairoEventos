using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Aplicacion.Eventos;
using Reportes.Dominio.ModelosLectura;
using Reportes.Infraestructura.Persistencia;

namespace Reportes.Aplicacion.Consumers;

public class UsuarioAccionRealizadaConsumer : IConsumer<UsuarioAccionRealizada>
{
    private readonly ReportesMongoDbContext _mongoContext;
    private readonly ILogger<UsuarioAccionRealizadaConsumer> _logger;

    public UsuarioAccionRealizadaConsumer(ReportesMongoDbContext mongoContext, ILogger<UsuarioAccionRealizadaConsumer> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UsuarioAccionRealizada> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consumiendo auditoría de usuario: {UsuarioId} - {Accion}", message.UsuarioId, message.Accion);

        var log = new LogAuditoria
        {
            Timestamp = message.Fecha,
            TipoOperacion = message.Accion,
            Entidad = "HTTP_REQUEST",
            EntidadId = message.Path,
            Detalles = message.Datos,
            Usuario = message.UsuarioId.ToString(),
            Exitoso = true
        };

        await _mongoContext.LogsAuditoria.InsertOneAsync(log);

        // También al historial unificado
        var historial = new ElementoHistorial
        {
            Id = Guid.NewGuid(),
            UsuarioId = message.UsuarioId,
            Tipo = "Seguridad",
            Descripcion = $"Acción de usuario: {message.Accion} en {message.Path}",
            Fecha = message.Fecha,
            Metadata = new { message.Path, message.Datos }
        };

        await _mongoContext.Timeline.InsertOneAsync(historial);
    }
}
