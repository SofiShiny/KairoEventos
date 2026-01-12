using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Repositorios;
using AutoMapper;
using System.Linq;

namespace Eventos.Aplicacion.Queries;

public class ObtenerEventosSugeridosQueryHandler : IQueryHandler<ObtenerEventosSugeridosQuery, Resultado<IEnumerable<EventoDto>>>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IMapper _mapper;

    public ObtenerEventosSugeridosQueryHandler(IRepositorioEvento repositorioEvento, IMapper mapper)
    {
        _repositorioEvento = repositorioEvento;
        _mapper = mapper;
    }

    public async Task<Resultado<IEnumerable<EventoDto>>> Handle(ObtenerEventosSugeridosQuery request, CancellationToken cancellationToken)
    {
        var eventos = await _repositorioEvento.ObtenerEventosPublicadosAsync(cancellationToken);
        
        var query = eventos.AsQueryable();

        // Filtrar por fecha si se solicita (por defecto eventos futuros)
        var fechaMinima = request.FechaDesde ?? DateTime.UtcNow;
        query = query.Where(e => e.FechaInicio >= fechaMinima);

        // Filtrar por categoría si se solicita
        if (!string.IsNullOrWhiteSpace(request.Categoria))
        {
            query = query.Where(e => e.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase));
        }

        // Tomar el top solicitado
        var sugeridos = query
            .OrderBy(e => e.FechaInicio)
            .Take(request.Top)
            .ToList();

        // Fallback: Si no hay sugerencias de la categoría, tomar eventos generales futuros
        if (!sugeridos.Any() && !string.IsNullOrWhiteSpace(request.Categoria))
        {
            sugeridos = eventos
                .Where(e => e.FechaInicio >= fechaMinima)
                .OrderBy(e => e.FechaInicio)
                .Take(request.Top)
                .ToList();
        }

        var lista = _mapper.Map<IEnumerable<EventoDto>>(sugeridos);
        return Resultado<IEnumerable<EventoDto>>.Exito(lista);
    }
}
