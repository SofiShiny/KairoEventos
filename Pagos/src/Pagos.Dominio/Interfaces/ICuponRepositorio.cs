using Pagos.Dominio.Entidades;

namespace Pagos.Dominio.Interfaces;

public interface ICuponRepositorio
{
    Task<Cupon?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<Cupon?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Cupon>> ObtenerPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<List<Cupon>> ObtenerGlobalesAsync(CancellationToken cancellationToken = default);
    Task<List<Cupon>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Cupon cupon, CancellationToken cancellationToken = default);
    Task AgregarVariosAsync(List<Cupon> cupones, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Cupon cupon, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}
