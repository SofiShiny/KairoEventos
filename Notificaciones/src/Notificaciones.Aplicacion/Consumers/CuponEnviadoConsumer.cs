using MassTransit;
using Microsoft.Extensions.Logging;
using Notificaciones.Dominio.ContratosExternos;
using Notificaciones.Dominio.Interfaces;

namespace Notificaciones.Aplicacion.Consumers;

public class CuponEnviadoConsumer : IConsumer<CuponEnviadoEvento>
{
    private readonly IServicioEmail _servicioEmail;
    private readonly INotificadorRealTime _notificador;
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly ILogger<CuponEnviadoConsumer> _logger;

    public CuponEnviadoConsumer(
        IServicioEmail servicioEmail,
        INotificadorRealTime notificador,
        IServicioUsuarios servicioUsuarios,
        ILogger<CuponEnviadoConsumer> logger)
    {
        _servicioEmail = servicioEmail;
        _notificador = notificador;
        _servicioUsuarios = servicioUsuarios;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CuponEnviadoEvento> context)
    {
        var evento = context.Message;
        _logger.LogInformation("Procesando cupón enviado: {Codigo} para usuario {UsuarioId}", 
            evento.Codigo, evento.UsuarioDestinatarioId);

        // 1. Notificación Real-Time (SignalR)
        await _notificador.EnviarMensajeUsuarioAsync(
            evento.UsuarioDestinatarioId.ToString(), 
            $"¡Nuevo cupón recibido: {evento.Codigo}!");

        // 2. Obtener Email del Usuario
        var emailDestino = await _servicioUsuarios.ObtenerEmailUsuarioAsync(evento.UsuarioDestinatarioId);

        if (string.IsNullOrEmpty(emailDestino))
        {
            _logger.LogWarning("No se pudo enviar email para el cupón {Codigo} porque no se encontró el correo del usuario {UsuarioId}", 
                evento.Codigo, evento.UsuarioDestinatarioId);
            return;
        }

        // 3. Notificación vía Email
        string displayValor = evento.TipoDescuento == "Porcentaje" ? $"{evento.Valor}%" : $"{evento.Valor:C}";
        
        string cuerpoHtml = $@"
            <html>
                <body style='font-family: sans-serif; text-align: center; padding: 20px;'>
                    <h1 style='color: #E91E63;'>¡Tienes un Regalo!</h1>
                    <p>Hola, el Organizador te ha enviado un cupón de descuento especial.</p>
                    <div style='background-color: #f8f9fa; border: 2px dashed #E91E63; padding: 20px; margin: 20px auto; display: inline-block;'>
                        <span style='font-size: 24px; font-weight: bold; color: #333;'>CÓDIGO: {evento.Codigo}</span>
                    </div>
                    <p style='font-size: 18px;'>Descuento: <b>{displayValor}</b></p>
                    <p style='color: #666;'>Válido hasta: {evento.FechaExpiracion:dd/MM/yyyy}</p>
                    <br/>
                    <p>¡No pierdas la oportunidad de usarlo en tu próxima compra!</p>
                </body>
            </html>";

        try
        {
            await _servicioEmail.EnviarEmailAsync(
                emailDestino, 
                "¡Tienes un Regalo! - Código de Descuento", 
                cuerpoHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email de cupón para {UsuarioId}", evento.UsuarioDestinatarioId);
        }
    }
}
