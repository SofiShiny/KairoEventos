using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Dominio.Repositorios
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Usuario?> ObtenerPorUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<Usuario?> ObtenerPorCorreoAsync(Correo correo, CancellationToken cancellationToken = default);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Usuario>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
        Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task<bool> ExisteUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExisteCorreoAsync(Correo correo, CancellationToken cancellationToken = default);
    }
}
