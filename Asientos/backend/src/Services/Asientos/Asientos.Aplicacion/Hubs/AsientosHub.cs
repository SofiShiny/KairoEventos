using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Asientos.Aplicacion.Hubs;

public class AsientosHub : Hub
{
    public async Task JoinEvento(string eventoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Evento_{eventoId}");
    }

    public async Task LeaveEvento(string eventoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Evento_{eventoId}");
    }
}
