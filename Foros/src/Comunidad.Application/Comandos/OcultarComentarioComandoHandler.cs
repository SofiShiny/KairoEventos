using Comunidad.Domain.Repositorios;
using MediatR;

namespace Comunidad.Application.Comandos;

public class OcultarComentarioComandoHandler : IRequestHandler<OcultarComentarioComando, Unit>
{
    private readonly IComentarioRepository _comentarioRepository;

    public OcultarComentarioComandoHandler(IComentarioRepository comentarioRepository)
    {
        _comentarioRepository = comentarioRepository;
    }

    public async Task<Unit> Handle(OcultarComentarioComando request, CancellationToken cancellationToken)
    {
        var comentario = await _comentarioRepository.ObtenerPorIdAsync(request.ComentarioId);
        if (comentario == null)
        {
            throw new InvalidOperationException($"El comentario con ID {request.ComentarioId} no existe");
        }

        comentario.Ocultar();
        await _comentarioRepository.ActualizarAsync(comentario);

        return Unit.Value;
    }
}
