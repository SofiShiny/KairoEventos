namespace Entradas.Dominio.Excepciones;

/// <summary>
/// Excepción lanzada cuando un evento no existe o no está disponible
/// </summary>
public class EventoNoDisponibleException : DominioException
{
    public Guid EventoId { get; }

    public EventoNoDisponibleException(Guid eventoId, string message) : base(message)
    {
        EventoId = eventoId;
    }

    public EventoNoDisponibleException(Guid eventoId, string message, Exception innerException) : base(message, innerException)
    {
        EventoId = eventoId;
    }
}