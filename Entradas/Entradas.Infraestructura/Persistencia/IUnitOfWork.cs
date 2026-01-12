namespace Entradas.Infraestructura.Persistencia;

/// <summary>
/// Interface para el patrón Unit of Work
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Inicia una nueva transacción
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda todos los cambios pendientes
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si hay una transacción activa
    /// </summary>
    bool HasActiveTransaction { get; }
}