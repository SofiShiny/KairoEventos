namespace Reportes.Aplicacion.Interfaces;

public interface INotificadorDashboard
{
    Task NotificarMetricasAsync(object metricas);
    Task NotificarVentasRecientesAsync(object ventas);
}
