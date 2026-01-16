using Comunidad.Application.DTOs;
using Comunidad.Domain.Repositorios;
using MediatR;

namespace Comunidad.Application.Consultas;

public class ObtenerComentariosQueryHandler : IRequestHandler<ObtenerComentariosQuery, List<ComentarioDto>>
{
    private readonly IComentarioRepository _comentarioRepository;
    private readonly IForoRepository _foroRepository;

    public ObtenerComentariosQueryHandler(
        IComentarioRepository comentarioRepository,
        IForoRepository foroRepository)
    {
        _comentarioRepository = comentarioRepository;
        _foroRepository = foroRepository;
    }

    public async Task<List<ComentarioDto>> Handle(ObtenerComentariosQuery request, CancellationToken cancellationToken)
    {
        var foro = await _foroRepository.ObtenerPorEventoIdAsync(request.EventoId);
        if (foro == null)
        {
            return new List<ComentarioDto>();
        }

        var comentarios = await _comentarioRepository.ObtenerPorForoIdAsync(foro.Id);

        // Filtrar solo comentarios visibles
        return comentarios
            .Where(c => c.EsVisible)
            .Select(c => new ComentarioDto
            {
                Id = c.Id,
                ForoId = c.ForoId,
                UsuarioId = c.UsuarioId,
                Contenido = c.Contenido,
                FechaCreacion = c.FechaCreacion,
                Respuestas = c.Respuestas
                    .Where(r => r.EsVisible) // Volvemos a filtrar aquí para no complicar el frontend
                    .Select(r => new RespuestaDto
                    {
                        // Buscamos el índice original en la lista NO filtrada para mantener la consistencia
                        // del ID determinista si el ID real es Guid.Empty
                        Id = (r.Id == Guid.Empty) 
                            ? Guid.Parse($"00000000-0000-0000-0000-{c.Respuestas.IndexOf(r):D12}") 
                            : r.Id,
                        UsuarioId = r.UsuarioId,
                        Contenido = r.Contenido,
                        FechaCreacion = r.FechaCreacion
                    }).ToList()
            })
            .ToList();
    }
}
