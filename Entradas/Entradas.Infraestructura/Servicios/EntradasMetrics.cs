using System.Diagnostics.Metrics;
using Entradas.Dominio.Interfaces;

namespace Entradas.Infraestructura.Servicios;

/// <summary>
/// Implementación de métricas para el servicio de entradas
/// </summary>
public class EntradasMetrics : IEntradasMetrics
{
    private readonly Meter _meter;
    private readonly Counter<int> _entradasCreadasCounter;
    private readonly Histogram<double> _creacionDurationHistogram;
    private readonly Counter<int> _validacionExternaErrorCounter;
    private readonly Histogram<double> _servicioExternoDurationHistogram;
    private readonly Counter<int> _pagosConfirmadosCounter;
    private readonly Histogram<double> _healthCheckDurationHistogram;

    public EntradasMetrics(Meter meter)
    {
        _meter = meter;

        // Contadores
        _entradasCreadasCounter = _meter.CreateCounter<int>(
            "entradas_creadas_total",
            "entradas",
            "Número total de entradas creadas");

        _validacionExternaErrorCounter = _meter.CreateCounter<int>(
            "validacion_externa_errores_total",
            "errores",
            "Número total de errores en validaciones externas");

        _pagosConfirmadosCounter = _meter.CreateCounter<int>(
            "pagos_confirmados_total",
            "pagos",
            "Número total de pagos confirmados procesados");

        // Histogramas
        _creacionDurationHistogram = _meter.CreateHistogram<double>(
            "entrada_creacion_duration_ms",
            "ms",
            "Duración de creación de entradas en milisegundos");

        _servicioExternoDurationHistogram = _meter.CreateHistogram<double>(
            "servicio_externo_duration_ms",
            "ms",
            "Duración de llamadas a servicios externos en milisegundos");

        _healthCheckDurationHistogram = _meter.CreateHistogram<double>(
            "health_check_duration_ms",
            "ms",
            "Duración de health checks en milisegundos");
    }

    public void IncrementEntradasCreadas(string eventoId, string estado)
    {
        _entradasCreadasCounter.Add(1, new KeyValuePair<string, object?>("evento_id", eventoId),
                                       new KeyValuePair<string, object?>("estado", estado));
    }

    public void RecordCreacionDuration(double durationMs, string resultado)
    {
        _creacionDurationHistogram.Record(durationMs, new KeyValuePair<string, object?>("resultado", resultado));
    }

    public void IncrementValidacionExternaError(string servicio, string tipoError)
    {
        _validacionExternaErrorCounter.Add(1, new KeyValuePair<string, object?>("servicio", servicio),
                                              new KeyValuePair<string, object?>("tipo_error", tipoError));
    }

    public void RecordServicioExternoDuration(string servicio, double durationMs, string resultado)
    {
        _servicioExternoDurationHistogram.Record(durationMs, 
            new KeyValuePair<string, object?>("servicio", servicio),
            new KeyValuePair<string, object?>("resultado", resultado));
    }

    public void IncrementPagosConfirmados(string resultado)
    {
        _pagosConfirmadosCounter.Add(1, new KeyValuePair<string, object?>("resultado", resultado));
    }

    public void RecordHealthCheckResult(string checkName, string resultado, double durationMs)
    {
        _healthCheckDurationHistogram.Record(durationMs,
            new KeyValuePair<string, object?>("check_name", checkName),
            new KeyValuePair<string, object?>("resultado", resultado));
    }
}