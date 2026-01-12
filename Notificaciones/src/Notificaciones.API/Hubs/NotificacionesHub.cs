using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Notificaciones.API.Hubs;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real
/// </summary>
[Authorize]
public class NotificacionesHub : Hub
{
    private readonly ILogger<NotificacionesHub> _logger;

    public NotificacionesHub(ILogger<NotificacionesHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se conecta al Hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation(
            "Usuario {UserId} conectado con ConnectionId {ConnectionId}", 
            userId, 
            connectionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Se ejecuta cuando un cliente se desconecta del Hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation(
            "Usuario {UserId} desconectado. ConnectionId {ConnectionId}", 
            userId, 
            connectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "Error en desconexión de usuario {UserId}", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Método que el cliente puede llamar para confirmar que está escuchando
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }
}
