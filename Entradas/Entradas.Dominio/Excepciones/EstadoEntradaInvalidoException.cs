namespace Entradas.Dominio.Excepciones;

/// <summary>
/// Excepción lanzada cuando se intenta realizar una operación inválida sobre el estado de una entrada
/// </summary>
public class EstadoEntradaInvalidoException : DominioException
{
    public EstadoEntradaInvalidoException(string message) : base(message)
    {
    }

    public EstadoEntradaInvalidoException(string message, Exception innerException) : base(message, innerException)
    {
    }
}