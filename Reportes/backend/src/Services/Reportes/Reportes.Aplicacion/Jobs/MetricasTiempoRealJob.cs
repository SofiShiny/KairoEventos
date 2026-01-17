using Reportes.Aplicacion.Interfaces;
using Reportes.Dominio.Repositorios;
using Microsoft.Extensions.Logging;

namespace Reportes.Aplicacion.Jobs;

public class MetricasTiempoRealJob
{
    private readonly INotificadorDashboard _notificador;
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<MetricasTiempoRealJob> _logger;

    public MetricasTiempoRealJob(
        INotificadorDashboard notificador,
        IRepositorioReportesLectura repositorio,
        ILogger<MetricasTiempoRealJob> logger)
    {
        _notificador = notificador;
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task ActualizarDashboardAsync()
    {
        try 
        {
            _logger.LogInformation("Actualizando dashboard en tiempo real...");

            var fechaHoy = DateTime.UtcNow.Date;
            
            // 1. Obtener ventas de hoy (todos los eventos)
            var reportesHoy = await _repositorio.ObtenerVentasPorRangoAsync(fechaHoy, fechaHoy);
            
            var totalVentas = reportesHoy.Sum(r => r.TotalIngresos);
            var totalEntradas = reportesHoy.Sum(r => r.CantidadReservas);
            
            // 2. Obtener métricas generales para contar eventos activos
            var eventosMetricas = await _repositorio.ObtenerTodasMetricasAsync();
            var eventosActivos = eventosMetricas.Count; // O filtrar por estado si existiera propiedad Activo

            var metricas = new
            {
                TotalVentasDia = totalVentas,
                EntradasVendidas = totalEntradas,
                EventosActivos = eventosActivos,
                UsuariosOnline = 2, // Hardcoded por requerimiento de usuario (prev. 0)
                Timestamp = DateTime.UtcNow
            };

            await _notificador.NotificarMetricasAsync(metricas);

            // TODO: Implementar obtención real de ventas recientes (requiere nuevo método en repositorio)
            // Por ahora enviamos lista vacía para evitar mostrar datos simulados falsos que confunden al usuario.
            var ventasRecientes = new List<object>();
            
            await _notificador.NotificarVentasRecientesAsync(ventasRecientes);
            
            _logger.LogInformation("Dashboard actualizado via SignalR. Ventas: {Ventas}, Entradas: {Entradas}", totalVentas, totalEntradas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando dashboard en tiempo real");
        }
    }
}
