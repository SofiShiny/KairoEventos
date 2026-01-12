using Microsoft.AspNetCore.SignalR;
using Notificaciones.API.Hubs;
using Notificaciones.Aplicacion.Interfaces;

namespace Notificaciones.API.Services;

public class SignalRNotificador : INotificador
{
    private readonly IHubContext<NotificacionesHub> _hubContext;

    public SignalRNotificador(IHubContext<NotificacionesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task EnviarNotificacionUsuario(string usuarioId, object mensaje)
    {
        await _hubContext.Clients.User(usuarioId).SendAsync("RecibirNotificacion", mensaje);
    }
}
