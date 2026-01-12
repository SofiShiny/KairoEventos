namespace Entradas.Dominio.Excepciones;

/// <summary>
/// Excepción lanzada cuando un servicio externo no está disponible
/// </summary>
public class ServicioExternoNoDisponibleException : DominioException
{
    public string NombreServicio { get; }

    public ServicioExternoNoDisponibleException(string nombreServicio, string message) : base(message)
    {
        NombreServicio = nombreServicio;
    }

    public ServicioExternoNoDisponibleException(string nombreServicio, string message, Exception innerException) : base(message, innerException)
    {
        NombreServicio = nombreServicio;
    }
}