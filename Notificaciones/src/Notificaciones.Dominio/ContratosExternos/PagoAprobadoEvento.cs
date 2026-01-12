namespace Pagos.Aplicacion.Eventos;

/// <summary>
/// Evento que se publica cuando un pago es aprobado
/// </summary>
public record PagoAprobadoEvento
{
    public string TransaccionId { get; init; } = string.Empty;
    public string OrdenId { get; init; } = string.Empty;
    public string UsuarioId { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public string? UrlFactura { get; init; }
}
