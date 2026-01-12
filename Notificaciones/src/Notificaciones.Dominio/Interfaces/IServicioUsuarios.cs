namespace Notificaciones.Dominio.Interfaces;

public interface IServicioUsuarios
{
    Task<string?> ObtenerEmailUsuarioAsync(Guid usuarioId);
}
