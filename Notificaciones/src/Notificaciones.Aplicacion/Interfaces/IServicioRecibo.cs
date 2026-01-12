using Pagos.Aplicacion.Eventos;

namespace Notificaciones.Aplicacion.Interfaces;

public interface IServicioRecibo
{
    byte[] GenerarPdfRecibo(PagoAprobadoEvento evento);
}
