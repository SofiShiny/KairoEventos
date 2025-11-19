using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Queries;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Queries;

public class ObtenerEventosQueryHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock = new();
 private readonly ObtenerEventosQueryHandler _handler;

 public ObtenerEventosQueryHandlerTests()
 {
 _handler = new ObtenerEventosQueryHandler(_repoMock.Object);
 }

 private Evento CrearEvento(string titulo)
 {
 var inicio = DateTime.UtcNow.AddDays(10);
 var fin = inicio.AddHours(5);
 var ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 return new Evento(titulo, "Desc", ubic, inicio, fin,50, "org-001");
 }

 [Fact]
 public async Task Handle_CuandoHayEventos_RegresaListaMapeada()
 {
 var e1 = CrearEvento("Evento1");
 var e2 = CrearEvento("Evento2");
 _repoMock.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
 .ReturnsAsync(new[] { e1, e2 });

 var res = await _handler.Handle(new ObtenerEventosQuery(), CancellationToken.None);

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

 var res = await _handler.Handle(new ObtenerEventosQuery(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Should().BeEmpty();
 }
}
