using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Queries;

public record ObtenerEventosPublicadosQuery() : IQuery<Resultado<IEnumerable<EventoDto>>>;