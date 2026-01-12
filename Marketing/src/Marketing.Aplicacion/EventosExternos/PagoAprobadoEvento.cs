namespace Marketing.Aplicacion.EventosExternos;

/// <summary>
/// Evento espejo proveniente de Pagos.API
/// Incluimos CodigoCupon para la lógica de Marketing
/// </summary>
public record PagoAprobadoEvento(
    Guid TransaccionId,
    Guid OrdenId,
    Guid UsuarioId,
    decimal Monto,
    string? CodigoCupon // El campo que nos interesa
);
