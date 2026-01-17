using Microsoft.AspNetCore.SignalR;

namespace Servicios.API.Hubs;

public class ServiciosHub : Hub
{
    public async Task JoinGroup(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task LeaveGroup(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }
}
