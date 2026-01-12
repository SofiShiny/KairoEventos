// IMPORTANTE: Este archivo usa el namespace del microservicio de origen (Eventos)
// para que MassTransit pueda deserializar correctamente los eventos publicados.
// NO cambiar el namespace a Reportes.Dominio.*

namespace Eventos.Dominio.EventosDeDominio;

/// <summary>
/// Contrato espejo del evento EventoPublicadoEventoDominio del microservicio de Eventos.
/// Se publica cuando un evento es publicado y está disponible para reservas.
/// </summary>
public record EventoPublicadoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; } = string.Empty;
    public DateTime FechaInicio { get; init; }
}

/// <summary>
/// Contrato espejo del evento AsistenteRegistradoEventoDominio del microservicio de Eventos.
/// Se publica cuando un usuario se registra como asistente a un evento.
/// </summary>
public record AsistenteRegistradoEventoDominio
{
    public Guid EventoId { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public string NombreUsuario { get; init; } = string.Empty;
}

/// <summary>
/// Contrato espejo del evento EventoCanceladoEventoDominio del microservicio de Eventos.
/// Se publica cuando un evento es cancelado.
/// </summary>
public record EventoCanceladoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; } = string.Empty;
}
