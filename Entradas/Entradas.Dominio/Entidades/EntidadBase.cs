namespace Entradas.Dominio.Entidades;

/// <summary>
/// Clase base para todas las entidades del dominio
/// </summary>
public abstract class EntidadBase
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Fecha de creación de la entidad
    /// </summary>
    public DateTime FechaCreacion { get; protected set; }
    
    /// <summary>
    /// Fecha de última actualización de la entidad
    /// </summary>
    public DateTime FechaActualizacion { get; protected set; }

    protected EntidadBase()
    {
        // Constructor sin parámetros para EF Core
        // NO inicializar propiedades aquí, EF Core las establecerá desde la BD
    }

    protected EntidadBase(Guid id)
    {
        Id = id;
        FechaCreacion = DateTime.UtcNow;
        FechaActualizacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza la fecha de modificación
    /// </summary>
    protected void ActualizarFechaModificacion()
    {
        FechaActualizacion = DateTime.UtcNow;
    }
}