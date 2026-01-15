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
        // Validar que el foro existe, si no, lo creamos on-the-fly
        var foro = await _foroRepository.ObtenerPorEventoIdAsync(request.ForoId);
        if (foro == null)
        {
            // Crear foro automáticamente si no existe (robusto ante falta de mensaje de publicación)
            foro = Foro.Crear(request.ForoId, "Foro del Evento");
            await _foroRepository.CrearAsync(foro);
        }

        var comentario = Comentario.Crear(foro.Id, request.UsuarioId, request.Contenido);
        await _comentarioRepository.CrearAsync(comentario);

        return comentario.Id;
    }
}
