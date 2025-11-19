using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Queries;

public record ObtenerEventosPublicadosQuery() : IQuery<Resultado<IEnumerable<EventoDto>>>;