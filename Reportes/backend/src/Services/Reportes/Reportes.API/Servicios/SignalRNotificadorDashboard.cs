using Microsoft.AspNetCore.SignalR;
using Reportes.Aplicacion.Interfaces;
using Reportes.API.Hubs;

namespace Reportes.API.Servicios;

public class SignalRNotificadorDashboard : INotificadorDashboard
{
    private readonly IHubContext<ReportesHub> _hubContext;

    public SignalRNotificadorDashboard(IHubContext<ReportesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarMetricasAsync(object metricas)
    {
        await _hubContext.Clients.Group("Dashboard").SendAsync("ReceiveMetricas", metricas);
    }

    public async Task NotificarVentasRecientesAsync(object ventas)
    {
        await _hubContext.Clients.Group("Dashboard").SendAsync("ReceiveVentasRecientes", ventas);
    }
}
