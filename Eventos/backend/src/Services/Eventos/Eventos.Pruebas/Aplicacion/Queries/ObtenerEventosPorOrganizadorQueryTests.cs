using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion.DTOs;
using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Queries;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosPorOrganizadorQueryTests
{
 private readonly string _orgId;
 private readonly ObtenerEventosPorOrganizadorQuery _query;

 public ObtenerEventosPorOrganizadorQueryTests()
 {
 _orgId = "org-123";
 _query = new ObtenerEventosPorOrganizadorQuery(_orgId);
 }

 [Fact]
 public void GuardaOrganizadorId()
 {
 _query.OrganizadorId.Should().Be(_orgId);
 }

 [Fact]
 public void Implementa_IQueryResultadoEnumerableEventoDto()
 {
 (_query is IQuery<Resultado<IEnumerable<EventoDto>>>).Should().BeTrue();
 }
}
