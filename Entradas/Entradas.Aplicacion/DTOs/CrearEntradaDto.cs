namespace Entradas.Aplicacion.DTOs;

/// <summary>
/// DTO para la solicitud de creación de una nueva entrada
/// </summary>
/// <param name="EventoId">Identificador único del evento</param>
/// <param name="UsuarioId">Identificador único del usuario</param>
/// <param name="AsientoId">Identificador único del asiento (opcional para entradas generales)</param>
/// <param name="Cupones">Lista opcional de códigos de cupones de descuento</param>
public record CrearEntradaDto(
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    List<string>? Cupones = null
);