using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;

namespace Usuarios.Dominio.Servicios
{
    public interface IServicioKeycloak
    {
        Task<string> CrearUsuarioAsync(Usuario usuario, string password, CancellationToken cancellationToken = default);
        Task ActualizarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task DesactivarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
        Task AsignarRolAsync(string usuarioId, Rol rol, CancellationToken cancellationToken = default);
    }
}
