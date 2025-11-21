using Eventos.Dominio.Entidades;

namespace Eventos.Dominio.Repositorios;

public interface IRepositorioEvento
{
    Task<Evento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Evento>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Evento>> ObtenerEventosPublicadosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Evento>> ObtenerEventosPorOrganizadorAsync(string organizadorId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Evento evento, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Evento evento, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}
