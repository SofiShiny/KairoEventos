namespace Notificaciones.Dominio.Interfaces;

public interface IServicioEmail
{
    Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml, byte[]? adjunto = null, string? nombreAdjunto = null);
}
