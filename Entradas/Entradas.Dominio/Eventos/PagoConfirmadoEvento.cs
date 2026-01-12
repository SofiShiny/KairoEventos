namespace Entradas.Dominio.Eventos;

/// <summary>
/// Evento que se recibe cuando un pago es confirmado por el sistema de pagos
/// </summary>
public record PagoConfirmadoEvento
{
    public Guid EntradaId { get; init; }
    public Guid TransaccionId { get; init; }
    public decimal MontoConfirmado { get; init; }
    public DateTime FechaPago { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
}