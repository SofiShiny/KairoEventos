using System.Threading.Tasks;

namespace Notificaciones.Aplicacion.Interfaces;

public interface INotificador
{
    Task EnviarNotificacionUsuario(string usuarioId, object mensaje);
}
