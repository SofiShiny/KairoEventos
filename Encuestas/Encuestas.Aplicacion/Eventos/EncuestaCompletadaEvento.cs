namespace Encuestas.Aplicacion.Eventos;

public record EncuestaCompletadaEvento(
    Guid RespuestaId,
    Guid EncuestaId,
    Guid UsuarioId,
    Guid EventoId,
    DateTime Fecha
);
