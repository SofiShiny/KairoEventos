using MediatR;

namespace Comunidad.Application.Comandos;

public record CrearComentarioComando(
    Guid ForoId,
    Guid UsuarioId,
    string Contenido
) : IRequest<Guid>;
