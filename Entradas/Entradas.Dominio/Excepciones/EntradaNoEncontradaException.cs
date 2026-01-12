namespace Entradas.Dominio.Excepciones;

/// <summary>
/// Excepción lanzada cuando no se encuentra una entrada específica
/// </summary>
public class EntradaNoEncontradaException : DominioException
{
    public Guid EntradaId { get; }

    public EntradaNoEncontradaException(Guid entradaId, string message) : base(message)
    {
        EntradaId = entradaId;
    }

    public EntradaNoEncontradaException(Guid entradaId, string message, Exception innerException) : base(message, innerException)
    {
        EntradaId = entradaId;
    }
}