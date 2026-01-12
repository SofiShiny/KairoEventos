namespace Pagos.Aplicacion.Eventos;

public record PagoAprobadoEvento(Guid TransaccionId, Guid OrdenId, Guid UsuarioId, decimal Monto, string UrlFactura);
