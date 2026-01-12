using Comunidad.Domain.Entidades;

namespace Comunidad.Domain.Repositorios;

public interface IForoRepository
{
    Task<Foro?> ObtenerPorEventoIdAsync(Guid eventoId);
    Task CrearAsync(Foro foro);
    Task<bool> ExistePorEventoIdAsync(Guid eventoId);
}
