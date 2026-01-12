using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using MediatR;

namespace Comunidad.Application.Comandos;

public class CrearComentarioComandoHandler : IRequestHandler<CrearComentarioComando, Guid>
{
    private readonly IComentarioRepository _comentarioRepository;
    private readonly IForoRepository _foroRepository;

    public CrearComentarioComandoHandler(
        IComentarioRepository comentarioRepository,
        IForoRepository foroRepository)
    {
        _comentarioRepository = comentarioRepository;
        _foroRepository = foroRepository;
    }

    public async Task<Guid> Handle(CrearComentarioComando request, CancellationToken cancellationToken)
    {
        // Validar que el foro existe
        var foro = await _foroRepository.ObtenerPorEventoIdAsync(request.ForoId);
        if (foro == null)
        {
            throw new InvalidOperationException($"El foro con ID {request.ForoId} no existe");
        }

        var comentario = Comentario.Crear(request.ForoId, request.UsuarioId, request.Contenido);
        await _comentarioRepository.CrearAsync(comentario);

        return comentario.Id;
    }
}
