using Eventos.Aplicacion.Queries;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosPublicadosQueryHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock = new();
 private readonly ObtenerEventosPublicadosQueryHandler _handler;

 public ObtenerEventosPublicadosQueryHandlerTests()
 {
 _handler = new ObtenerEventosPublicadosQueryHandler(_repoMock.Object);
 }

 private Evento CrearPublicado(string titulo)
 {
 var inicio = DateTime.UtcNow.AddDays(5);
 var fin = inicio.AddHours(2);
 var ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 var ev = new Evento(titulo, "Desc", ubic, inicio, fin,10, "org-001");
 ev.Publicar();
 return ev;
 }

 [Fact]
 public async Task Handle_RegresaSoloPublicados()
 {
 var pub = CrearPublicado("Pub1");
 _repoMock.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(new[] { pub });
 var res = await _handler.Handle(new ObtenerEventosPublicadosQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().HaveCount(1);
 res.Valor.First().Estado.Should().Be(EstadoEvento.Publicado.ToString());
 }

 [Fact]
 public async Task Handle_SinPublicados_RegresaListaVacia()
 {
 _repoMock.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(Array.Empty<Evento>());
 var res = await _handler.Handle(new ObtenerEventosPublicadosQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().BeEmpty();
 }
}
