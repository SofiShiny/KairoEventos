using MediatR;
using Recomendaciones.Dominio.Entidades;
using Recomendaciones.Dominio.Repositorios;

namespace Recomendaciones.Aplicacion.Queries;

public record ObtenerRecomendacionesQuery(Guid UsuarioId) : IRequest<IEnumerable<EventoProyeccion>>;

public class ObtenerRecomendacionesQueryHandler : IRequestHandler<ObtenerRecomendacionesQuery, IEnumerable<EventoProyeccion>>
{
    private readonly IRepositorioRecomendaciones _repositorio;

    public ObtenerRecomendacionesQueryHandler(IRepositorioRecomendaciones repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<EventoProyeccion>> Handle(ObtenerRecomendacionesQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtener afinidades del usuario
        var afinidades = await _repositorio.ObtenerAfinidadesPorUsuarioAsync(request.UsuarioId);
        
        // 2. Cold Start: Si no hay historial, devolver los próximos 5 eventos
        if (!afinidades.Any())
        {
            var todosFuturos = await _repositorio.ObtenerEventosFuturosAsync();
            return todosFuturos.Take(5);
        }

        // 3. Obtener las top 3 categorías con mayor puntaje
        var topCategorias = afinidades
            .OrderByDescending(a => a.Puntuacion)
            .Take(3)
            .Select(a => a.Categoria)
            .ToList();

        // 4. Buscar eventos en esas categorías
        var recomendados = await _repositorio.ObtenerEventosPorCategoriasAsync(topCategorias);

        // 5. Ordenar por relevancia (categoría con más puntos primero) y fecha
        // Creamos un diccionario para búsqueda rápida de peso de categoría
        var pesosCategorias = afinidades.ToDictionary(a => a.Categoria, a => a.Puntuacion);

        return recomendados
            .OrderByDescending(e => pesosCategorias.GetValueOrDefault(e.Categoria, 0))
            .ThenBy(e => e.Fecha)
            .Take(10); // Máximo 10 recomendaciones
    }
}
