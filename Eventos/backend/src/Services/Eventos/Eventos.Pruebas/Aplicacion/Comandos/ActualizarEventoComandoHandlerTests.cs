using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using System.Linq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class ActualizarEventoComandoHandlerTests
{
 private readonly Mock<IRepositorioEvento> _repo;
 private readonly ActualizarEventoComandoHandler _handler;
 private readonly Evento _eventoBase;
 private readonly DateTime _inicio;
 private readonly DateTime _fin;

 public ActualizarEventoComandoHandlerTests()
 {
 _repo = new Mock<IRepositorioEvento>();
 _handler = new ActualizarEventoComandoHandler(_repo.Object);
 _inicio = DateTime.UtcNow.AddDays(5);
 _fin = _inicio.AddHours(2);
 _eventoBase = new Evento("T","D", new Ubicacion("L","D","C","R","0","P"), _inicio, _fin,10,"org");
 }

 // Helper builders
 private ActualizarEventoComando CmdTitulo(string? titulo) => new(_eventoBase.Id, titulo, null, null, null, null, null);
 private ActualizarEventoComando CmdUbicacion(UbicacionDto u) => new(_eventoBase.Id, null, null, u, null, null, null);
 private ActualizarEventoComando CmdFechas(DateTime? nuevaInicio=null, DateTime? nuevaFin=null) => new(_eventoBase.Id, null, null, null, nuevaInicio, nuevaFin, null);
 private ActualizarEventoComando CmdMaximo(int? max) => new(_eventoBase.Id, null, null, null, null, null, max);
 private ActualizarEventoComando CmdSinCambios() => new(_eventoBase.Id, null, null, null, null, null, null);

 [Fact]
 public async Task Handle_EventoNoExiste_Falla()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
 var res = await _handler.Handle(new ActualizarEventoComando(Guid.NewGuid(), null, null, null, null, null, null), CancellationToken.None);
 res.EsExitoso.Should().BeFalse();
 }

 [Fact]
 public async Task Handle_ActualizaTitulo()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var res = await _handler.Handle(CmdTitulo("Nuevo"), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Titulo.Should().Be("Nuevo");
 }

 [Fact]
 public async Task Handle_ActualizaUbicacion()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var nueva = new UbicacionDto{ NombreLugar="NL", Direccion="ND", Ciudad="C", Pais="P" };
 var res = await _handler.Handle(CmdUbicacion(nueva), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Ubicacion!.NombreLugar.Should().Be("NL");
 }

 [Fact]
 public async Task Handle_SoloFechaInicio_Modifica()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var nuevoInicio = _inicio.AddHours(2);
 var res = await _handler.Handle(CmdFechas(nuevoInicio, null), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.FechaInicio.Should().Be(nuevoInicio);
 }

 [Fact]
 public async Task Handle_ModificaFechaFin_Y_Maximo()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var nuevaFin = _eventoBase.FechaFin.AddHours(2);
 var res = await _handler.Handle(new ActualizarEventoComando(_eventoBase.Id, null, null, null, null, nuevaFin,10), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.FechaFin.Should().Be(nuevaFin);
 res.Valor!.MaximoAsistentes.Should().Be(10);
 }

 [Fact]
 public async Task Handle_UbicacionFallback_CamposNulos()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var parcial = new UbicacionDto{ NombreLugar="LugarNuevo", Direccion=null, Ciudad=null, Region=null, CodigoPostal=null, Pais="PaisNuevo" };
 var res = await _handler.Handle(CmdUbicacion(parcial), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Ubicacion!.NombreLugar.Should().Be("LugarNuevo");
 res.Valor.Ubicacion!.Pais.Should().Be("PaisNuevo");
 }

 [Fact]
 public async Task Handle_UbicacionFallback_NombreYPaisNulos()
 {
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var parcial = new UbicacionDto{ NombreLugar=null, Direccion="DirNueva", Ciudad="CiudadNueva", Region="RegionNueva", CodigoPostal="CPNueva", Pais=null };
 var res = await _handler.Handle(CmdUbicacion(parcial), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Ubicacion!.NombreLugar.Should().Be("L");
 res.Valor.Ubicacion!.Pais.Should().Be("P");
 }

 [Fact]
 public async Task Handle_MapeaAsistentes()
 {
 _eventoBase.Publicar();
 _eventoBase.RegistrarAsistente("u1","N1","a1@b.com");
 _eventoBase.RegistrarAsistente("u2","N2","a2@b.com");
 _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoBase.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoBase);
 _repo.Setup(r=>r.ActualizarAsync(_eventoBase, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
 var res = await _handler.Handle(CmdSinCambios(), CancellationToken.None);
 res.EsExitoso.Should().BeTrue();
 res.Valor!.Asistentes.Should().NotBeNull();
 res.Valor.Asistentes!.Count().Should().Be(2);
 }
}
