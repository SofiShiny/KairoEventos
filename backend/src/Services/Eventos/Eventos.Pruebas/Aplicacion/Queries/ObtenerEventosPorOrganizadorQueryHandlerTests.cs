using Eventos.Aplicacion.Queries;
using Eventos.Aplicacion;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;
using AutoMapper;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosPorOrganizadorQueryHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock;
 private readonly ObtenerEventosPorOrganizadorQueryHandler _handler;
 private readonly IMapper _mapper;
 private readonly DateTime _inicio;
 private readonly Ubicacion _ubic;

 public ObtenerEventosPorOrganizadorQueryHandlerTests()
 {
 _repoMock = new Mock<IRepositorioEvento>();
 var cfg = new MapperConfiguration(c => c.AddProfile(new EventoMappingProfile()));
 _mapper = cfg.CreateMapper();
 _handler = new ObtenerEventosPorOrganizadorQueryHandler(_repoMock.Object, _mapper);
 _inicio = DateTime.UtcNow.AddDays(3);
 _ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 }

 private Evento Build(string org, string titulo)
 {
 var fin = _inicio.AddHours(1);
 return new Evento(titulo, "Desc", _ubic, _inicio, fin,5, org);
 }
 private ObtenerEventosPorOrganizadorQuery BuildQuery(string org) => new(org);

 [Fact]
 public async Task Handle_FiltraPorOrganizador()
 {
 var ev1 = Build("org-1", "A");
 var ev2 = Build("org-1", "B");
 _repoMock.Setup(r => r.ObtenerEventosPorOrganizadorAsync("org-1", It.IsAny<CancellationToken>()))
 .ReturnsAsync(new[] { ev1, ev2 });
 var res = await _handler.Handle(BuildQuery("org-1"), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().HaveCount(2);
 res.Valor.Should().NotContain(x => x.Titulo == "C");
 }

 [Fact]
 public async Task Handle_SinEventos_Vacio()
 {
 _repoMock.Setup(r => r.ObtenerEventosPorOrganizadorAsync("org-x", It.IsAny<CancellationToken>()))
 .ReturnsAsync(Array.Empty<Evento>());
 var res = await _handler.Handle(BuildQuery("org-x"), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().BeEmpty();
 }
}
