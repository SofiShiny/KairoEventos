using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Repositorios;
using FluentAssertions;
using Moq;
using System.Linq;
using Xunit;
using Eventos.Dominio.Enumeraciones;
using AutoMapper;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosPublicadosQueryHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock;
 private readonly ObtenerEventosPublicadosQueryHandler _handler;
 private readonly IMapper _mapper;
 private readonly DateTime _inicio;
 private readonly DateTime _fin;
 private readonly Ubicacion _ubic;

 public ObtenerEventosPublicadosQueryHandlerTests()
 {
 _repoMock = new Mock<IRepositorioEvento>();
 var cfg = new MapperConfiguration(c => c.AddProfile(new EventoMappingProfile()));
 _mapper = cfg.CreateMapper();
 _handler = new ObtenerEventosPublicadosQueryHandler(_repoMock.Object, _mapper);
 _inicio = DateTime.UtcNow.AddDays(5);
 _fin = _inicio.AddHours(2);
 _ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 }

 private Evento BuildPublicado(string titulo)
 {
 var ev = new Evento(titulo, "Desc", _ubic, _inicio, _fin,10, "org-001");
 ev.Publicar();
 return ev;
 }
 private ObtenerEventosPublicadosQuery BuildQuery() => new();

 [Fact]
 public async Task Handle_RegresaSoloPublicados()
 {
 var pub = BuildPublicado("Pub1");
 _repoMock.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(new[] { pub });
 var res = await _handler.Handle(BuildQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().HaveCount(1);
 res.Valor.First(). Estado.Should().Be(EstadoEvento.Publicado.ToString());
 }

 [Fact]
 public async Task Handle_SinPublicados_Vacio()
 {
 _repoMock.Setup(r => r.ObtenerEventosPublicadosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(Array.Empty<Evento>());
 var res = await _handler.Handle(BuildQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().BeEmpty();
 }
}
