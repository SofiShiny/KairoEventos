using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;
using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventoPorIdQueryTests
{
 private readonly Guid _id;
 private readonly ObtenerEventoPorIdQuery _query;

 public ObtenerEventoPorIdQueryTests()
 {
 _id = Guid.NewGuid();
 _query = new ObtenerEventoPorIdQuery(_id);
 }

 [Fact]
 public void GuardaId()
 {
 _query.EventoId.Should().Be(_id);
 }

 [Fact]
 public void ImplementaIQueryResultadoEventoDto()
 {
 (_query is IQuery<Resultado<EventoDto>>).Should().BeTrue();
 }
}
