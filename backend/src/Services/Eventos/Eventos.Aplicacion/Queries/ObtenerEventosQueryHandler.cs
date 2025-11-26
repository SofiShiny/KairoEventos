using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Repositorios;
using AutoMapper;

namespace Eventos.Aplicacion.Queries;

public class ObtenerEventosQueryHandler : IQueryHandler<ObtenerEventosQuery, Resultado<IEnumerable<EventoDto>>>
{
 private readonly IRepositorioEvento _repositorioEvento;
 private readonly IMapper _mapper;

 public ObtenerEventosQueryHandler(IRepositorioEvento repositorioEvento, IMapper mapper)
 {
 _repositorioEvento = repositorioEvento;
 _mapper = mapper;
 }

 public async Task<Resultado<IEnumerable<EventoDto>>> Handle(ObtenerEventosQuery request, CancellationToken cancellationToken)
 {
 var eventos = await _repositorioEvento.ObtenerTodosAsync(cancellationToken);
 var lista = _mapper.Map<IEnumerable<EventoDto>>(eventos);
 return Resultado<IEnumerable<EventoDto>>.Exito(lista);
 }
}