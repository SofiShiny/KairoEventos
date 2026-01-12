namespace Entradas.Dominio.Excepciones;

/// <summary>
/// Excepción base para todas las excepciones del dominio
/// </summary>
public abstract class DominioException : Exception
{
    protected DominioException(string message) : base(message)
    {
    }

    protected DominioException(string message, Exception innerException) : base(message, innerException)
    {
    }
}