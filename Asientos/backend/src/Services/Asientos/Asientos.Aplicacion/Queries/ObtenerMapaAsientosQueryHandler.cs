using MediatR;
using Asientos.Dominio.Repositorios;

namespace Asientos.Aplicacion.Queries;

public class ObtenerMapaAsientosQueryHandler : IRequestHandler<ObtenerMapaAsientosQuery, MapaAsientosDto?>
{
    private readonly IRepositorioMapaAsientos _repo;
    
    public ObtenerMapaAsientosQueryHandler(IRepositorioMapaAsientos repo) => _repo = repo;
    
    public async Task<MapaAsientosDto?> Handle(ObtenerMapaAsientosQuery request, CancellationToken cancellationToken)
    {
        var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken);
        if (mapa == null) return null;
        
        var asientos = mapa.Asientos
            .Select(a => new AsientoDto(a.Id, a.Fila, a.Numero, a.Categoria.Nombre, a.Categoria.PrecioBase ?? 0, a.Reservado))
            .OrderBy(x => x.Fila)
            .ThenBy(x => x.Numero)
            .ToList();
            
        var categorias = mapa.Categorias
            .Select(c => new CategoriaDto(c.Nombre, c.PrecioBase, c.TienePrioridad))
            .OrderByDescending(c => c.TienePrioridad)
            .ToList();
            
        return new MapaAsientosDto(mapa.Id, mapa.EventoId, categorias, asientos);
    }
}
