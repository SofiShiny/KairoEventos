using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Repositorios;
using AutoMapper;

namespace Eventos.Aplicacion.Queries;

public class ObtenerEventosPorOrganizadorQueryHandler : IQueryHandler<ObtenerEventosPorOrganizadorQuery, Resultado<IEnumerable<EventoDto>>>
{
 private readonly IRepositorioEvento _repositorioEvento;
 private readonly IMapper _mapper;

 public ObtenerEventosPorOrganizadorQueryHandler(IRepositorioEvento repositorioEvento, IMapper mapper)
 {
 _repositorioEvento = repositorioEvento;
 _mapper = mapper;
 }

 public async Task<Resultado<IEnumerable<EventoDto>>> Handle(ObtenerEventosPorOrganizadorQuery request, CancellationToken cancellationToken)
 {
 var eventos = await _repositorioEvento.ObtenerEventosPorOrganizadorAsync(request.OrganizadorId, cancellationToken);
 var lista = _mapper.Map<IEnumerable<EventoDto>>(eventos);
 return Resultado<IEnumerable<EventoDto>>.Exito(lista);
 }
}