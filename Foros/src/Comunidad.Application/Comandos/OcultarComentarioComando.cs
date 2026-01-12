using MediatR;

namespace Comunidad.Application.Comandos;

public record OcultarComentarioComando(Guid ComentarioId) : IRequest<Unit>;
