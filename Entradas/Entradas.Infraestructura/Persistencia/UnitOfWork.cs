using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Entradas.Infraestructura.Persistencia;

/// <summary>
/// Implementación del patrón Unit of Work usando Entity Framework Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly EntradasDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed = false;

    public UnitOfWork(EntradasDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa.");
        }

        try
        {
            _logger.LogDebug("Iniciando nueva transacción");
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogDebug("Transacción iniciada exitosamente: {TransactionId}", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar transacción");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para confirmar.");
        }

        try
        {
            _logger.LogDebug("Confirmando transacción: {TransactionId}", _currentTransaction.TransactionId);
            
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            
            _logger.LogDebug("Transacción confirmada exitosamente: {TransactionId}", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar transacción: {TransactionId}", _currentTransaction?.TransactionId);
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogWarning("Intento de rollback sin transacción activa");
            return;
        }

        try
        {
            _logger.LogDebug("Revirtiendo transacción: {TransactionId}", _currentTransaction.TransactionId);
            
            await _currentTransaction.RollbackAsync(cancellationToken);
            
            _logger.LogDebug("Transacción revertida exitosamente: {TransactionId}", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revertir transacción: {TransactionId}", _currentTransaction?.TransactionId);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Guardando cambios en el contexto");
            
            var result = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Se guardaron {Cantidad} cambios exitosamente", result);
            
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Conflicto de concurrencia al guardar cambios");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al guardar cambios");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al guardar cambios");
            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    _logger.LogWarning("Disposing UnitOfWork con transacción activa. Realizando rollback automático.");
                    _currentTransaction.Rollback();
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante dispose de UnitOfWork");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}