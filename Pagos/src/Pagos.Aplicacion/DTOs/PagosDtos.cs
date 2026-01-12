namespace Pagos.Aplicacion.DTOs;

public record CrearPagoDto(Guid OrdenId, Guid UsuarioId, decimal Monto, string Tarjeta);
public record TransaccionDto(Guid Id, Guid OrdenId, decimal Monto, string Estado, string? UrlFactura);
