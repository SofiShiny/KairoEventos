using System;

namespace Entradas.Dominio.Eventos;

/// <summary>
/// Contrato espejo del evento publicado por Entradas.API
/// </summary>
public record EntradaCreadaEvento
{
    public Guid EntradaId { get; init; }
    public Guid EventoId { get; init; }
    public Guid UsuarioId { get; init; }
    public decimal Monto { get; init; }
    public decimal MontoFinal => Monto;
    public decimal MontoDescuento { get; init; }
    public string? CuponesAplicados { get; init; }
    public DateTime FechaCreacion { get; init; }
}
