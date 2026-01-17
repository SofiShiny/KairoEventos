using System;
using System.Collections.Generic;

namespace Entradas.Dominio.Eventos;

public record EntradaPagadaEvento
{
    public Guid OrdenId { get; init; }
    public Guid EventoId { get; init; }
    public decimal MontoTotal { get; init; }
    public List<Guid> AsientosIds { get; init; } = new();
}
