using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;
using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Queries;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosPublicadosQueryTests
{
 private readonly ObtenerEventosPublicadosQuery _query;

 public ObtenerEventosPublicadosQueryTests()
 {
 _query = new ObtenerEventosPublicadosQuery();
 }

 [Fact]
 public void Instancia_NoEsNula()
 {
 _query.Should().NotBeNull();
 }

 [Fact]
 public void Implementa_IQueryResultadoEnumerableEventoDto()
 {
 (_query is IQuery<Resultado<IEnumerable<EventoDto>>>).Should().BeTrue();
 }
}
