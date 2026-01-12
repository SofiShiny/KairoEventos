using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notificaciones.Dominio.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Notificaciones.Infraestructura.Servicios;

public class ConfiguracionEmail
{
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NombreEmisor { get; set; } = string.Empty;
    public string EmailEmisor { get; set; } = string.Empty;
    public bool UsarSsl { get; set; }
}

[ExcludeFromCodeCoverage]
public class ServicioEmailSmtp : IServicioEmail
{
    private readonly ConfiguracionEmail _config;
    private readonly ILogger<ServicioEmailSmtp> _logger;
    private readonly Func<ISmtpClient> _clientFactory;

    public ServicioEmailSmtp(
        IOptions<ConfiguracionEmail> config, 
        ILogger<ServicioEmailSmtp> logger,
        Func<ISmtpClient>? clientFactory = null)
    {
        _config = config.Value;
        _logger = logger;
        _clientFactory = clientFactory ?? (() => new SmtpClient());
    }

    public async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml, byte[]? adjunto = null, string? nombreAdjunto = null)
    {
        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_config.NombreEmisor, _config.EmailEmisor));
            mensaje.To.Add(new MailboxAddress("", destinatario));
            mensaje.Subject = asunto;

            var bodyBuilder = new BodyBuilder { HtmlBody = cuerpoHtml };
            
            if (adjunto != null && !string.IsNullOrEmpty(nombreAdjunto))
            {
                bodyBuilder.Attachments.Add(nombreAdjunto, adjunto);
            }

            mensaje.Body = bodyBuilder.ToMessageBody();

            using var cliente = _clientFactory();
            
            await cliente.ConnectAsync(_config.Host, _config.Puerto, _config.UsarSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            
            if (!string.IsNullOrEmpty(_config.Usuario))
            {
                await cliente.AuthenticateAsync(_config.Usuario, _config.Password);
            }

            await cliente.SendAsync(mensaje);
            await cliente.DisconnectAsync(true);

            _logger.LogInformation("Email enviado exitosamente a {Destinatario} con/sin adjunto", destinatario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al enviar email a {Destinatario}", destinatario);
        }
    }
}
