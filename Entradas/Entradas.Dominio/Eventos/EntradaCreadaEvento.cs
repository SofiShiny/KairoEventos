namespace Entradas.Dominio.Eventos;

/// <summary>
/// Evento que se publica cuando una entrada es creada exitosamente
/// </summary>
public record EntradaCreadaEvento
{
    public Guid EntradaId { get; init; }
    public Guid EventoId { get; init; }
    public Guid UsuarioId { get; init; }
    public decimal Monto { get; init; }
    public decimal MontoDescuento { get; init; }
    public string? CuponesAplicados { get; init; }
    public DateTime FechaCreacion { get; init; }
    public string CodigoQr { get; init; } = string.Empty;
    public string? NombreUsuario { get; init; }
    public string? Email { get; init; }
}