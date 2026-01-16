using MediatR;

namespace Comunidad.Application.Comandos;

public record OcultarComentarioComando(Guid ComentarioId, Guid? RespuestaId = null) : IRequest<Unit>;
