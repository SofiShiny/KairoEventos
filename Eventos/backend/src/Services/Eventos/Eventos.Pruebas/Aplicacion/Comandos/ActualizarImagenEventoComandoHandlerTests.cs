using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Interfaces;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using BloquesConstruccion.Aplicacion.Comun;
using FluentAssertions;
using Moq;
using Xunit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class ActualizarImagenEventoComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly Mock<IGestorArchivos> _gestor;
    private readonly ActualizarImagenEventoComandoHandler _handler;
    private readonly Guid _eventId;
    private readonly Evento _evento;

    public ActualizarImagenEventoComandoHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>();
        _gestor = new Mock<IGestorArchivos>();
        _handler = new ActualizarImagenEventoComandoHandler(_repo.Object, _gestor.Object);
        
        _eventId = Guid.NewGuid();
        var ubicacion = new Ubicacion("L", "D", "C", "R", "0", "P");
        _evento = new Evento("T", "D", ubicacion, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "org");
        typeof(Evento).GetProperty("Id")!.SetValue(_evento, _eventId);
    }

    [Fact]
    public async Task Handle_EventoExiste_SubeImagenYActualiza()
    {
        // Arrange
        var stream = new MemoryStream();
        var cmd = new ActualizarImagenEventoComando(_eventId, stream, "test.jpg");
        
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_evento);
        _gestor.Setup(g => g.SubirImagenAsync(stream, "test.jpg", "imagenes")).ReturnsAsync("/imagenes/test.jpg");
        _repo.Setup(r => r.ActualizarAsync(_evento, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var res = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        res.Valor.Should().Be("/imagenes/test.jpg");
        _evento.UrlImagen.Should().Be("/imagenes/test.jpg");
        _repo.Verify(r => r.ActualizarAsync(_evento, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EventoTieneImagenPrevia_LaBorraAntesDeSubir()
    {
        // Arrange
        typeof(Evento).GetProperty("UrlImagen")!.SetValue(_evento, "/old/path.jpg");
        var stream = new MemoryStream();
        var cmd = new ActualizarImagenEventoComando(_eventId, stream, "new.jpg");
        
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_evento);
        _gestor.Setup(g => g.BorrarImagenAsync("/old/path.jpg")).Returns(Task.CompletedTask);
        _gestor.Setup(g => g.SubirImagenAsync(stream, "new.jpg", "imagenes")).ReturnsAsync("/imagenes/new.jpg");

        // Act
        await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        _gestor.Verify(g => g.BorrarImagenAsync("/old/path.jpg"), Times.Once);
    }

    [Fact]
    public async Task Handle_EventoNoExiste_Falla()
    {
        // Arrange
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
        var cmd = new ActualizarImagenEventoComando(_eventId, new MemoryStream(), "t.jpg");

        // Act
        var res = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("No se encontró");
    }

    [Fact]
    public async Task Handle_Excepcion_Falla()
    {
        // Arrange
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Fail"));
        var cmd = new ActualizarImagenEventoComando(_eventId, new MemoryStream(), "t.jpg");

        // Act
        var res = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("Error al actualizar imagen");
    }
}
