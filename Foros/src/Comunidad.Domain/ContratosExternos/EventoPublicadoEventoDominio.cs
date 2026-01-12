// TRUCO DEL NAMESPACE: Usar el namespace original del emisor para que MassTransit lo reconozca
namespace Eventos.Domain.Events;

public record EventoPublicadoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; } = string.Empty;
    public DateTime FechaInicio { get; init; }
}
