using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class EliminarEventoComandoHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock = new();
 private readonly EliminarEventoComandoHandler _handler;

 public EliminarEventoComandoHandlerTests()
 {
 _handler = new EliminarEventoComandoHandler(_repoMock.Object);
 }

 private Evento CrearEvento()
 {
 var inicio = DateTime.UtcNow.AddDays(5);
 var fin = inicio.AddHours(3);
 var ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 return new Evento("Titulo", "Desc", ubic, inicio, fin,20, "org-001");
 }

 [Fact]
 public async Task Handle_EliminaEvento_DevuelveExito()
 {
 var evento = CrearEvento();
 _repoMock.Setup(r => r.ObtenerPorIdAsync(evento.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evento);
 _repoMock.Setup(r => r.EliminarAsync(evento.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var res = await _handler.Handle(new EliminarEventoComando(evento.Id), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 _repoMock.Verify(r => r.EliminarAsync(evento.Id, It.IsAny<CancellationToken>()), Times.Once);
 }

 [Fact]
 public async Task Handle_EventoNoExiste_RetornaFalla()
 {
 _repoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
 var res = await _handler.Handle(new EliminarEventoComando(Guid.NewGuid()), CancellationToken.None);
 res.EsExitoso.Should().BeFalse();
 res.Error.Should().Contain("no encontrado");
 }
}
