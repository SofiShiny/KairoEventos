using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.Enumeraciones;

namespace Eventos.Aplicacion.Queries;

public class ObtenerEventosPublicadosQueryHandler : IQueryHandler<ObtenerEventosPublicadosQuery, Resultado<IEnumerable<EventoDto>>>
{
 private readonly IRepositorioEvento _repositorioEvento;

 public ObtenerEventosPublicadosQueryHandler(IRepositorioEvento repositorioEvento)
 {
 _repositorioEvento = repositorioEvento;
 }

 public async Task<Resultado<IEnumerable<EventoDto>>> Handle(ObtenerEventosPublicadosQuery request, CancellationToken cancellationToken)
 {
 var eventos = await _repositorioEvento.ObtenerEventosPublicadosAsync(cancellationToken);
 var lista = eventos.Select(e => new EventoDto
 {
 Id = e.Id,
 Titulo = e.Titulo,
 Descripcion = e.Descripcion,
 Ubicacion = new UbicacionDto
 {
 NombreLugar = e.Ubicacion.NombreLugar,
 Direccion = e.Ubicacion.Direccion,
 Ciudad = e.Ubicacion.Ciudad,
 Region = e.Ubicacion.Region,
 CodigoPostal = e.Ubicacion.CodigoPostal,
 Pais = e.Ubicacion.Pais
 },
 FechaInicio = e.FechaInicio,
 FechaFin = e.FechaFin,
 MaximoAsistentes = e.MaximoAsistentes,
 ConteoAsistentesActual = e.ConteoAsistentesActual,
 Estado = e.Estado.ToString(),
 OrganizadorId = e.OrganizadorId,
 CreadoEn = e.CreadoEn
 });
 return Resultado<IEnumerable<EventoDto>>.Exito(lista);
 }
}