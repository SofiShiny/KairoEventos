using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;

namespace Entradas.Dominio.Interfaces;

/// <summary>
/// Interface del repositorio para la gestión de entradas
/// </summary>
public interface IRepositorioEntradas
{
    /// <summary>
    /// Obtiene una entrada por su ID
    /// </summary>
    /// <param name="id">ID de la entrada</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entrada si existe, null en caso contrario</returns>
    Task<Entrada?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una entrada por su código QR
    /// </summary>
    /// <param name="codigoQr">Código QR de la entrada</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entrada si existe, null en caso contrario</returns>
    Task<Entrada?> ObtenerPorCodigoQrAsync(string codigoQr, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entradas de un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entradas del usuario</returns>
    Task<IEnumerable<Entrada>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entradas de un evento específico
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entradas del evento</returns>
    Task<IEnumerable<Entrada>> ObtenerPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene entradas filtradas por estado
    /// </summary>
    /// <param name="estado">Estado de las entradas de buscar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entradas con el estado especificado</returns>
    Task<IEnumerable<Entrada>> ObtenerPorEstadoAsync(EstadoEntrada estado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda una nueva entrada o actualiza una existente
    /// </summary>
    /// <param name="entrada">Entrada a guardar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entrada guardada</returns>
    Task<Entrada> GuardarAsync(Entrada entrada, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una entrada (eliminación lógica)
    /// </summary>
    /// <param name="id">ID de la entrada a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se eliminó correctamente, false si no se encontró</returns>
    Task<bool> EliminarAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una entrada con el código QR especificado
    /// </summary>
    /// <param name="codigoQr">Código QR a verificar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe, false en caso contrario</returns>
    Task<bool> ExisteCodigoQrAsync(string codigoQr, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el conteo de entradas por evento y estado
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="estado">Estado de las entradas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entradas</returns>
    Task<int> ContarPorEventoYEstadoAsync(Guid eventoId, EstadoEntrada estado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las entradas que están en estado Pendiente y han superado el tiempo de expiración
    /// </summary>
    /// <param name="fechaLimite">Fecha y hora límite para considerar una reserva vencida</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entradas pendientes vencidas</returns>
    Task<IEnumerable<Entrada>> ObtenerPendientesVencidasAsync(DateTime fechaLimite, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una entrada activa para un asiento específico
    /// </summary>
    /// <param name="asientoId">ID del asiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entrada encontrada o null</returns>
    Task<Entrada?> ObtenerActivaPorAsientoAsync(Guid asientoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entradas del sistema
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista completa de entradas</returns>
    Task<IEnumerable<Entrada>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entradas asociadas a un OrdenId (para compras múltiples)
    /// </summary>
    /// <param name="ordenId">ID de la orden de compra</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de entradas asociadas a la orden</returns>
    Task<List<Entrada>> ObtenerPorOrdenIdAsync(Guid ordenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza múltiples entradas en una sola operación
    /// </summary>
    /// <param name="entradas">Lista de entradas a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    Task ActualizarRangoAsync(IEnumerable<Entrada> entradas, CancellationToken cancellationToken = default);
}