namespace Entradas.Dominio.Interfaces;

/// <summary>
/// Interface para verificar la disponibilidad de asientos
/// </summary>
public interface IVerificadorAsientos
{
    /// <summary>
    /// Verifica si un asiento está disponible para un evento específico
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="asientoId">ID del asiento (null para entradas generales)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el asiento está disponible, false en caso contrario</returns>
    Task<bool> AsientoDisponibleAsync(Guid eventoId, Guid? asientoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserva temporalmente un asiento para permitir el proceso de compra
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="asientoId">ID del asiento a reservar</param>
    /// <param name="usuarioId">ID del usuario que reserva</param>
    /// <param name="duracion">Duración de la reserva temporal</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    Task ReservarAsientoTemporalAsync(Guid eventoId, Guid asientoId, Guid usuarioId, TimeSpan duracion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene información de un asiento específico
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="asientoId">ID del asiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del asiento o null si no existe</returns>
    Task<AsientoInfo?> ObtenerInfoAsientoAsync(Guid eventoId, Guid asientoId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Información básica de un asiento
/// </summary>
public record AsientoInfo(
    Guid Id,
    Guid MapaId,
    string Seccion,
    int Fila,
    int Numero,
    bool EstaDisponible,
    decimal PrecioAdicional
);