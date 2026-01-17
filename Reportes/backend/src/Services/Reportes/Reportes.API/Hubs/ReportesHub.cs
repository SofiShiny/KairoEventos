using Microsoft.AspNetCore.SignalR;

namespace Reportes.API.Hubs;

public class ReportesHub : Hub
{
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");
    }

    public async Task LeaveDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Dashboard");
    }
}
