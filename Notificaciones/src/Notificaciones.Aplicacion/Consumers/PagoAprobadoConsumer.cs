using MassTransit;
using Microsoft.Extensions.Logging;
using Notificaciones.Aplicacion.Interfaces;
using Pagos.Aplicacion.Eventos;
using Notificaciones.Dominio.Interfaces;
using System;
using System.Threading.Tasks;

namespace Notificaciones.Aplicacion.Consumers;

/// <summary>
/// Consumer que escucha eventos de pagos aprobados y envía notificaciones
/// </summary>
public class PagoAprobadoConsumer : IConsumer<PagoAprobadoEvento>
{
    private readonly INotificador _notificador;
    private readonly IServicioEmail _servicioEmail;
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IServicioRecibo _servicioRecibo;
    private readonly ILogger<PagoAprobadoConsumer> _logger;

    public PagoAprobadoConsumer(
        INotificador notificador,
        IServicioEmail servicioEmail,
        IServicioUsuarios servicioUsuarios,
        IServicioRecibo servicioRecibo,
        ILogger<PagoAprobadoConsumer> logger)
    {
        _notificador = notificador;
        _servicioEmail = servicioEmail;
        _servicioUsuarios = servicioUsuarios;
        _servicioRecibo = servicioRecibo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoAprobadoEvento> context)
    {
        var mensaje = context.Message;
        
        _logger.LogInformation(
            "📬 Pago aprobado recibido. TransaccionId: {TransaccionId}, OrdenId: {OrdenId}, UsuarioId: {UsuarioId}, Monto: {Monto}",
            mensaje.TransaccionId,
            mensaje.OrdenId,
            mensaje.UsuarioId,
            mensaje.Monto);

        // 1. Enviar notificación en tiempo real (SignalR)
        try
        {
            var notificacion = new
            {
                tipo = "pago_aprobado",
                titulo = "¡Pago Confirmado! 🎉",
                mensaje = $"Tu pago de ${mensaje.Monto:N2} ha sido procesado exitosamente.",
                transaccionId = mensaje.TransaccionId,
                ordenId = mensaje.OrdenId,
                monto = mensaje.Monto,
                urlFactura = mensaje.UrlFactura,
                timestamp = DateTime.UtcNow
            };

            await _notificador.EnviarNotificacionUsuario(mensaje.UsuarioId, notificacion);
            _logger.LogInformation("✅ Notificación SignalR enviada al usuario {UsuarioId}", mensaje.UsuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar notificación SignalR al usuario {UsuarioId}", mensaje.UsuarioId);
        }

        // 2. Generar PDF y Enviar Correo Electrónico
        try
        {
            if (Guid.TryParse(mensaje.UsuarioId, out var usuarioGuid))
            {
                var emailUsuario = await _servicioUsuarios.ObtenerEmailUsuarioAsync(usuarioGuid);

                if (!string.IsNullOrEmpty(emailUsuario))
                {
                    // Generar el PDF
                    _logger.LogInformation("📄 Generando PDF para la transacción {TxId}", mensaje.TransaccionId);
                    byte[] pdfBytes = _servicioRecibo.GenerarPdfRecibo(mensaje);

                    string asunto = "Tu Comprobante de Pago - Kairo Eventos";
                    string cuerpo = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h1 style='color: #2563eb;'>¡Gracias por tu compra!</h1>
                            <p>Hola,</p>
                            <p>Tu pago ha sido procesado correctamente. Adjunto a este correo encontrarás tu comprobante de pago oficial en formato PDF.</p>
                            <div style='background-color: #f3f4f6; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                                <p><strong>Monto:</strong> ${mensaje.Monto:N2}</p>
                                <p><strong>Orden:</strong> {mensaje.OrdenId}</p>
                                <p><strong>Transacción:</strong> {mensaje.TransaccionId}</p>
                            </div>
                            <p>Si tienes alguna duda, puedes contactarnos respondiendo a este correo.</p>
                            <br>
                            <p>Saludos,<br>El equipo de <strong>Kairo Eventos</strong></p>
                        </div>
                    ";

                    string nombreArchivo = $"Recibo_{mensaje.TransaccionId.ToString().Substring(0, 8)}.pdf";

                    await _servicioEmail.EnviarEmailAsync(emailUsuario, asunto, cuerpo, pdfBytes, nombreArchivo);
                    _logger.LogInformation("✉️ Email con recibo PDF enviado a {Email}", emailUsuario);
                }
                else
                {
                    _logger.LogWarning("⚠️ No se encontró email para el usuario {UsuarioId}", mensaje.UsuarioId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error crítico en el flujo de email/recibo para {UsuarioId}", mensaje.UsuarioId);
        }
    }
}
