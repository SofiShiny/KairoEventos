using Comunidad.Domain.Entidades;

namespace Comunidad.Domain.Repositorios;

public interface IComentarioRepository
{
    Task<List<Comentario>> ObtenerPorForoIdAsync(Guid foroId);
    Task<Comentario?> ObtenerPorIdAsync(Guid id);
    Task CrearAsync(Comentario comentario);
    Task ActualizarAsync(Comentario comentario);
}
