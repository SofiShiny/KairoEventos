using Microsoft.AspNetCore.SignalR;
using Servicios.API.Hubs;
using Hangfire;

namespace Servicios.API.Jobs;

public class NotificationJob
{
    private readonly IHubContext<ServiciosHub> _hubContext;

    public NotificationJob(IHubContext<ServiciosHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [JobDisplayName("Notificar actualización de servicio: {0}")]
    public async Task NotificarCambioEstado(string idServicioExterno, string nombreServicio, decimal nuevoPrecio, bool disponible)
    {
        var mensaje = disponible 
            ? $"🔥 ¡El servicio '{nombreServicio}' ya está disponible por ${nuevoPrecio}!" 
            : $"⚠️ El servicio '{nombreServicio}' se ha agotado.";

        Console.WriteLine($"[JOB] Enviando notificación SignalR: {mensaje}");

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new {
            idServicioExterno = idServicioExterno,
            nombre = nombreServicio,
            disponible = disponible,
            precio = nuevoPrecio,
            titulo = "Actualización de Servicio",
            mensaje = mensaje,
            tipo = disponible ? "success" : "warning",
            timestamp = DateTime.UtcNow
        });
        
        Console.WriteLine($"[JOB] Notificación enviada exitosamente.");
    }
}
