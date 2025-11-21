using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;
using Eventos.Pruebas.Shared;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class EliminarEventoComandoHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock;
 private readonly EliminarEventoComandoHandler _handler;
 private readonly Evento _evento;

 public EliminarEventoComandoHandlerTests()
 {
 _repoMock = new Mock<IRepositorioEvento>(MockBehavior.Strict);
 _handler = new EliminarEventoComandoHandler(_repoMock.Object);
 _evento = TestHelpers.BuildEvento();
 }

 private EliminarEventoComando Build(Guid id) => new(id);

 [Fact]
 public async Task Handle_EliminaEvento_Exito()
 {
 _repoMock.Setup(r => r.ObtenerPorIdAsync(_evento.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_evento);
 _repoMock.Setup(r => r.EliminarAsync(_evento.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var res = await _handler.Handle(Build(_evento.Id), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 _repoMock.VerifyAll();
 }

 [Fact]
 public async Task Handle_EventoNoExiste_Falla()
 {
 _repoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
 var res = await _handler.Handle(Build(Guid.NewGuid()), CancellationToken.None);
 res.EsExitoso.Should().BeFalse();
 res.Error.Should().Contain("no encontrado");
 _repoMock.Verify(r=>r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
 }
}
