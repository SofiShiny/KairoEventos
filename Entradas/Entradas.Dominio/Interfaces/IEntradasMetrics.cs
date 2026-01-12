namespace Entradas.Dominio.Interfaces;

/// <summary>
/// Interface para métricas del servicio de entradas
/// </summary>
public interface IEntradasMetrics
{
    /// <summary>
    /// Incrementa el contador de entradas creadas
    /// </summary>
    void IncrementEntradasCreadas(string eventoId, string estado);

    /// <summary>
    /// Registra la duración de creación de una entrada
    /// </summary>
    void RecordCreacionDuration(double durationMs, string resultado);

    /// <summary>
    /// Incrementa el contador de errores de validación externa
    /// </summary>
    void IncrementValidacionExternaError(string servicio, string tipoError);

    /// <summary>
    /// Registra la duración de llamadas a servicios externos
    /// </summary>
    void RecordServicioExternoDuration(string servicio, double durationMs, string resultado);

    /// <summary>
    /// Incrementa el contador de eventos de pago procesados
    /// </summary>
    void IncrementPagosConfirmados(string resultado);

    /// <summary>
    /// Registra métricas de health checks
    /// </summary>
    void RecordHealthCheckResult(string checkName, string resultado, double durationMs);
}