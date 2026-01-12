namespace Notificaciones.Dominio.Interfaces;

public interface INotificadorRealTime
{
    Task EnviarMensajeUsuarioAsync(string usuarioId, string mensaje);
}
