using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Queries;

public record ObtenerEventosQuery() : IQuery<Resultado<IEnumerable<EventoDto>>>;