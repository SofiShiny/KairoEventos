namespace Entradas.Dominio.Excepciones;

/// <summary>
/// Excepción lanzada cuando un asiento no está disponible
/// </summary>
public class AsientoNoDisponibleException : DominioException
{
    public Guid? AsientoId { get; }
    public Guid EventoId { get; }

    public AsientoNoDisponibleException(string message) : base(message)
    {
    }

    public AsientoNoDisponibleException(Guid eventoId, Guid? asientoId, string message) : base(message)
    {
        EventoId = eventoId;
        AsientoId = asientoId;
    }

    public AsientoNoDisponibleException(Guid eventoId, Guid? asientoId, string message, Exception innerException) : base(message, innerException)
    {
        EventoId = eventoId;
        AsientoId = asientoId;
    }
}