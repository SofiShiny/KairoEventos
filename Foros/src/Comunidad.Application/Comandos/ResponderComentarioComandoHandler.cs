using Comunidad.Domain.Repositorios;
using MediatR;

namespace Comunidad.Application.Comandos;

public class ResponderComentarioComandoHandler : IRequestHandler<ResponderComentarioComando, Unit>
{
    private readonly IComentarioRepository _comentarioRepository;

    public ResponderComentarioComandoHandler(IComentarioRepository comentarioRepository)
    {
        _comentarioRepository = comentarioRepository;
    }

    public async Task<Unit> Handle(ResponderComentarioComando request, CancellationToken cancellationToken)
    {
        var comentario = await _comentarioRepository.ObtenerPorIdAsync(request.ComentarioId);
        if (comentario == null)
        {
            throw new InvalidOperationException($"El comentario con ID {request.ComentarioId} no existe");
        }

        comentario.AgregarRespuesta(request.UsuarioId, request.Contenido);
        await _comentarioRepository.ActualizarAsync(comentario);

        return Unit.Value;
    }
}
