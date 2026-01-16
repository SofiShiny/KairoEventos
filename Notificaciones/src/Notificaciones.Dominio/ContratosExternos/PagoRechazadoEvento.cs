namespace Pagos.Aplicacion.Eventos;

/// <summary>
/// Evento que se publica cuando un pago es rechazado
/// </summary>
public record PagoRechazadoEvento
{
    public Guid TransaccionId { get; init; }
    public Guid OrdenId { get; init; }
    public Guid UsuarioId { get; init; }
    public string Motivo { get; init; } = string.Empty;
}
