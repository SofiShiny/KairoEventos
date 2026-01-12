namespace Notificaciones.Dominio.Contratos;

/// <summary>
/// Contrato espejo del evento publicado por Pagos.API
/// </summary>
public record PagoAprobadoEvento(
    Guid TransaccionId, 
    Guid OrdenId, 
    Guid UsuarioId, 
    decimal Monto, 
    string UrlFactura);
