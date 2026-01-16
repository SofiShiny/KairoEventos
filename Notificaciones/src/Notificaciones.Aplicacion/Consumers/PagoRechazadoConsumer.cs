using MassTransit;
using Microsoft.Extensions.Logging;
using Notificaciones.Aplicacion.Interfaces;
using Pagos.Aplicacion.Eventos;
using Notificaciones.Dominio.Interfaces;
using System;
using System.Threading.Tasks;

namespace Notificaciones.Aplicacion.Consumers;

/// <summary>
/// Consumer que escucha eventos de pagos rechazados y envía notificaciones
/// </summary>
public class PagoRechazadoConsumer : IConsumer<PagoRechazadoEvento>
{
    private readonly INotificador _notificador;
    private readonly ILogger<PagoRechazadoConsumer> _logger;

    public PagoRechazadoConsumer(
        INotificador notificador,
        ILogger<PagoRechazadoConsumer> logger)
    {
        _notificador = notificador;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoRechazadoEvento> context)
    {
        var mensaje = context.Message;
        
        _logger.LogWarning(
            "📬 Pago rechazado recibido. TransaccionId: {TransaccionId}, OrdenId: {OrdenId}, UsuarioId: {UsuarioId}, Motivo: {Motivo}",
            mensaje.TransaccionId,
            mensaje.OrdenId,
            mensaje.UsuarioId,
            mensaje.Motivo);

        // Enviar notificación en tiempo real (SignalR) con el texto solicitado por el usuario
        try
        {
            var notificacion = new
            {
                tipo = "pago_rechazado",
                titulo = "Pago fallido no procesado ⚠️",
                mensaje = $"Lo sentimos, tu pago no pudo ser procesado. Motivo: {mensaje.Motivo}",
                transaccionId = mensaje.TransaccionId,
                ordenId = mensaje.OrdenId,
                timestamp = DateTime.UtcNow
            };

            await _notificador.EnviarNotificacionUsuario(mensaje.UsuarioId.ToString(), notificacion);
            _logger.LogInformation("✅ Notificación de fallo SignalR enviada al usuario {UsuarioId}", mensaje.UsuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar notificación de fallo SignalR al usuario {UsuarioId}", mensaje.UsuarioId);
        }
    }
}
