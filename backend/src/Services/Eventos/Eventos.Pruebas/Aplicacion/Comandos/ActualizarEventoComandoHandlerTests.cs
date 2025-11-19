using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class ActualizarEventoComandoHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repoMock = new();
 private readonly ActualizarEventoComandoHandler _handler;

 public ActualizarEventoComandoHandlerTests()
 {
 _handler = new ActualizarEventoComandoHandler(_repoMock.Object);
 }

 private Evento CrearEvento()
 {
 var inicio = DateTime.UtcNow.AddDays(10);
 var fin = inicio.AddHours(2);
 var ubic = new Ubicacion("Lugar", "Dir", "Ciudad", "Region", "0000", "Pais");
 return new Evento("Titulo", "Desc", ubic, inicio, fin,10, "org-001");
 }

 [Fact]
 public async Task Handle_ActualizaCampos()
 {
 var evento = CrearEvento();
 _repoMock.Setup(r => r.ObtenerPorIdAsync(evento.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evento);
 _repoMock.Setup(r => r.ActualizarAsync(evento, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var cmd = new ActualizarEventoComando(evento.Id, "Nuevo", null, null, null, null, null);
 var res = await _handler.Handle(cmd, CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Titulo.Should().Be("Nuevo");
 _repoMock.Verify(r => r.ActualizarAsync(evento, It.IsAny<CancellationToken>()), Times.Once);
 }

 [Fact]
 public async Task Handle_EventoNoExiste_RetornaFalla()
 {
 _repoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
 var cmd = new ActualizarEventoComando(Guid.NewGuid(), "Nuevo", null, null, null, null, null);
 var res = await _handler.Handle(cmd, CancellationToken.None);
 res.EsExitoso.Should().BeFalse();
 res.Error.Should().Contain("no encontrado");
 }

 [Fact]
 public async Task Handle_ActualizaUbicacion()
 {
 var evento = CrearEvento();
 _repoMock.Setup(r => r.ObtenerPorIdAsync(evento.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evento);
 _repoMock.Setup(r => r.ActualizarAsync(evento, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var nuevaUbic = new UbicacionDto { NombreLugar = "LugarNuevo", Direccion = "DirNueva", Ciudad = "Ciudad", Region = "Region", CodigoPostal = "9999", Pais = "Pais" };
 var cmd = new ActualizarEventoComando(evento.Id, null, null, nuevaUbic, null, null, null);
 var res = await _handler.Handle(cmd, CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Ubicacion!.NombreLugar.Should().Be("LugarNuevo");
 }
}
