using MediatR;

namespace Comunidad.Application.Comandos;

public record ResponderComentarioComando(
    Guid ComentarioId,
    Guid UsuarioId,
    string Contenido
) : IRequest<Unit>;
