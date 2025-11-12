using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Queries;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventoPorIdQueryTests
{
 [Fact]
 public void Query_DeberiaGuardarEventoId()
 {
 var id = Guid.NewGuid();
 var query = new ObtenerEventoPorIdQuery(id);

 query.EventoId.Should().Be(id);
 }

 [Fact]
 public void Query_DeberiaImplementarIQueryResultadoEventoDto()
 {
 var query = new ObtenerEventoPorIdQuery(Guid.Empty);

 (query is BloquesConstruccion.Aplicacion.Queries.IQuery<Resultado<EventoDto>>).Should().BeTrue();
 }
}
