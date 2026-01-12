namespace Notificaciones.Dominio.ContratosExternos;

/// <summary>
/// Evento espejo del publicado por Marketing.API
/// </summary>
public record CuponEnviadoEvento(
    Guid UsuarioDestinatarioId,
    string Codigo,
    decimal Valor,
    string TipoDescuento,
    DateTime FechaExpiracion);
