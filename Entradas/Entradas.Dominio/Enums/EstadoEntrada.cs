namespace Entradas.Dominio.Enums;

/// <summary>
/// Estados posibles de una entrada en el sistema
/// </summary>
public enum EstadoEntrada
{
    /// <summary>
    /// La entrada está reservada temporalmente esperando el inicio del proceso de pago.
    /// No se considera una entrada "real" hasta que se pague.
    /// </summary>
    Reservada = 0,

    /// <summary>
    /// La entrada ha sido creada pero el pago aún no ha sido confirmado
    /// </summary>
    PendientePago = 1,
    
    /// <summary>
    /// El pago de la entrada ha sido confirmado
    /// </summary>
    Pagada = 2,
    
    /// <summary>
    /// La entrada ha sido cancelada
    /// </summary>
    Cancelada = 3,
    
    /// <summary>
    /// La entrada ha sido utilizada para acceder al evento
    /// </summary>
    Usada = 4
}