using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Jobs;

/// <summary>
/// Job de Hangfire que genera reportes consolidados diariamente.
/// Se ejecuta a las 2 AM para consolidar datos del día anterior.
/// </summary>
public class JobGenerarReportesConsolidados
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<JobGenerarReportesConsolidados> _logger;

    public JobGenerarReportesConsolidados(
        IRepositorioReportesLectura repositorio,
        ILogger<JobGenerarReportesConsolidados> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el job de consolidación de reportes.
    /// Consolida datos del día anterior agregando información de múltiples colecciones.
    /// </summary>
    public async Task EjecutarAsync()
    {
        var inicioEjecucion = DateTime.UtcNow;
        _logger.LogInformation("Iniciando job de consolidación de reportes a las {Timestamp}", inicioEjecucion);

        try
        {
            // Calcular período: día anterior completo
            var ayer = DateTime.UtcNow.Date.AddDays(-1);
            var periodoInicio = ayer;
            var periodoFin = ayer.AddDays(1).AddSeconds(-1);

            _logger.LogInformation(
                "Consolidando datos del período: {Inicio} a {Fin}", 
                periodoInicio, 
                periodoFin);

            // Obtener datos de ventas del día anterior
            var ventasDiarias = await _repositorio.ObtenerVentasPorRangoAsync(periodoInicio, periodoFin);
            
            // Obtener todas las métricas de eventos
            var todasMetricas = await _repositorio.ObtenerTodasMetricasAsync();
            
            // Filtrar métricas del día anterior
            var metricasPeriodo = todasMetricas
                .Where(m => m.FechaInicio.Date == ayer.Date)
                .ToList();

            // Calcular totales
            var totalIngresos = ventasDiarias.Sum(v => v.TotalIngresos);
            var totalReservas = ventasDiarias.Sum(v => v.CantidadReservas);
            var totalEventos = metricasPeriodo.Count;

            // Calcular promedio de asistencia por evento
            var promedioAsistencia = totalEventos > 0
                ? metricasPeriodo.Average(m => m.TotalAsistentes)
                : 0;

            // Calcular ingresos por categoría
            var ingresosPorCategoria = new Dictionary<string, decimal>();
            foreach (var venta in ventasDiarias)
            {
                foreach (var categoria in venta.ReservasPorCategoria)
                {
                    if (!ingresosPorCategoria.ContainsKey(categoria.Key))
                    {
                        ingresosPorCategoria[categoria.Key] = 0;
                    }
                    
                    // Calcular ingreso proporcional por categoría
                    // (simplificado: distribuir ingresos proporcionalmente)
                    var totalReservasVenta = venta.ReservasPorCategoria.Values.Sum();
                    if (totalReservasVenta > 0)
                    {
                        var proporcion = (decimal)categoria.Value / totalReservasVenta;
                        ingresosPorCategoria[categoria.Key] += venta.TotalIngresos * proporcion;
                    }
                }
            }

            // Crear reporte consolidado
            var reporte = new ReporteConsolidado
            {
                FechaConsolidacion = DateTime.UtcNow,
                PeriodoInicio = periodoInicio,
                PeriodoFin = periodoFin,
                TotalIngresos = totalIngresos,
                TotalReservas = totalReservas,
                TotalEventos = totalEventos,
                PromedioAsistenciaEvento = promedioAsistencia,
                IngresosPorCategoria = ingresosPorCategoria
            };

            // Guardar reporte consolidado
            await _repositorio.GuardarReporteConsolidadoAsync(reporte);

            var duracion = DateTime.UtcNow - inicioEjecucion;
            _logger.LogInformation(
                "Reporte consolidado generado exitosamente para {Fecha}. " +
                "Total ingresos: {Ingresos}, Total reservas: {Reservas}, Total eventos: {Eventos}. " +
                "Duración: {Duracion}ms",
                ayer.ToString("yyyy-MM-dd"),
                totalIngresos,
                totalReservas,
                totalEventos,
                duracion.TotalMilliseconds);

            // Registrar éxito en auditoría
            try
            {
                await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
                {
                    Timestamp = DateTime.UtcNow,
                    TipoOperacion = "ReporteGenerado",
                    Entidad = "ReporteConsolidado",
                    EntidadId = reporte.Id,
                    Detalles = $"Consolidación exitosa para {ayer:yyyy-MM-dd}. " +
                              $"Ingresos: {totalIngresos}, Reservas: {totalReservas}, Eventos: {totalEventos}",
                    Usuario = "Sistema",
                    Exitoso = true,
                    MensajeError = null
                });
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Error registrando auditoría de éxito, pero el job se completó correctamente");
            }
        }
        catch (Exception ex)
        {
            var duracion = DateTime.UtcNow - inicioEjecucion;
            _logger.LogError(
                ex, 
                "Error generando reporte consolidado. Duración antes del error: {Duracion}ms", 
                duracion.TotalMilliseconds);

            // Registrar fallo en auditoría
            try
            {
                await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
                {
                    Timestamp = DateTime.UtcNow,
                    TipoOperacion = "ErrorProcesamiento",
                    Entidad = "ReporteConsolidado",
                    EntidadId = string.Empty,
                    Detalles = "Error en job de consolidación",
                    Usuario = "Sistema",
                    Exitoso = false,
                    MensajeError = ex.Message
                });
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Error adicional registrando fallo en auditoría");
            }

            // Re-lanzar excepción para que Hangfire maneje el reintento
            throw;
        }
    }
}
