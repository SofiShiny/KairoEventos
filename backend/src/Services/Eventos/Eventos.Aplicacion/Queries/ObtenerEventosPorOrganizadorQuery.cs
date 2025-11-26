using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Queries;

public record ObtenerEventosPorOrganizadorQuery(string OrganizadorId) : IQuery<Resultado<IEnumerable<EventoDto>>>;