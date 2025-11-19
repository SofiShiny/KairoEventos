using Eventos.Aplicacion.Queries;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosPorOrganizadorQueryHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock = new();
 private readonly ObtenerEventosPorOrganizadorQueryHandler _handler;

 public ObtenerEventosPorOrganizadorQueryHandlerTests()
 {
 _handler = new ObtenerEventosPorOrganizadorQueryHandler(_repoMock.Object);
 }

 private Evento Crear(string org, string titulo)
 {
 var inicio = DateTime.UtcNow.AddDays(3);
 var fin = inicio.AddHours(1);
 var ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 return new Evento(titulo, "Desc", ubic, inicio, fin,5, org);
 }

 [Fact]
 public async Task Handle_FiltraPorOrganizador()
 {
 var ev1 = Crear("org-1", "A");
 var ev2 = Crear("org-1", "B");
 var evOtro = Crear("org-2", "C");
 _repoMock.Setup(r => r.ObtenerEventosPorOrganizadorAsync("org-1", It.IsAny<CancellationToken>()))
 .ReturnsAsync(new[] { ev1, ev2 });
 var res = await _handler.Handle(new ObtenerEventosPorOrganizadorQuery("org-1"), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().HaveCount(2);
 res.Valor.Should().NotContain(x => x.Titulo == "C");
 }

 [Fact]
 public async Task Handle_SinEventos_RegresaListaVacia()
 {
 _repoMock.Setup(r => r.ObtenerEventosPorOrganizadorAsync("org-x", It.IsAny<CancellationToken>()))
 .ReturnsAsync(Array.Empty<Evento>());
 var res = await _handler.Handle(new ObtenerEventosPorOrganizadorQuery("org-x"), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().BeEmpty();
 }
}
