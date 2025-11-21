using Eventos.Aplicacion.Queries;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosQueryHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock;
 private readonly ObtenerEventosQueryHandler _handler;
 private readonly DateTime _inicioBase;
 private readonly DateTime _finBase;
 private readonly Ubicacion _ubicacionBase;

 public ObtenerEventosQueryHandlerTests()
 {
 _repoMock = new Mock<IRepositorioEvento>();
 _handler = new ObtenerEventosQueryHandler(_repoMock.Object);
 _inicioBase = DateTime.UtcNow.AddDays(10);
 _finBase = _inicioBase.AddHours(5);
 _ubicacionBase = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 }

 private Evento BuildEvento(string titulo) => new(titulo, "Desc", _ubicacionBase, _inicioBase, _finBase,50, "org-001");
 private ObtenerEventosQuery BuildQuery() => new();

 [Fact]
 public async Task Handle_CuandoHayEventos_RegresaListaMapeada()
 {
 var e1 = BuildEvento("Evento1");
 var e2 = BuildEvento("Evento2");
 _repoMock.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(new[] { e1, e2 });
 var res = await _handler.Handle(BuildQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().HaveCount(2);
 res.Valor.Should().Contain(x => x.Titulo == "Evento1");
 res.Valor.Should().AllSatisfy(dto => dto.Ubicacion!.NombreLugar.Should().Be("Lugar"));
 }

 [Fact]
 public async Task Handle_SinEventos_RegresaListaVacia()
 {
 _repoMock.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(Array.Empty<Evento>());
 var res = await _handler.Handle(BuildQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().BeEmpty();
 }
}
