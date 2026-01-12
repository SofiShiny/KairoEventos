using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using MassTransit;
using Xunit;
using Eventos.Pruebas.Shared;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class PublicarEventoComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Mock<ILogger<PublicarEventoComandoHandler>> _logger;
    private readonly PublicarEventoComandoHandler _handler;
    private readonly Evento _evento;
    private readonly Guid _eventId;
    private readonly Evento _eventoPublicado;

    public PublicarEventoComandoHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>(MockBehavior.Strict);
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _logger = new Mock<ILogger<PublicarEventoComandoHandler>>();
        _handler = new PublicarEventoComandoHandler(_repo.Object, _publishEndpoint.Object, _logger.Object);
        _evento = TestHelpers.BuildEvento("Evento Peliculas", "Mejores peliculas",500, "organizador-001",30,48);
        _eventId = Guid.NewGuid();
        typeof(Evento).GetProperty("Id")!.SetValue(_evento, _eventId);
        _eventoPublicado = TestHelpers.BuildEvento("YaPublicado", "Desc",10, "org",5,2);
        _eventoPublicado.Publicar();
    }

    private PublicarEventoComando Cmd(Guid id) => new(id);

    [Fact]
    public async Task Handle_Valido_PublicaEvento()
    {
        // Arrange
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_evento);
        _repo.Setup(r=>r.ActualizarAsync(_evento, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var res = await _handler.Handle(Cmd(_eventId), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        _evento.Estado.Should().Be(EstadoEvento.Publicado);
        _evento.EventosDominio.Should().ContainSingle().Which.GetType().Name.Should().Contain("Publicado");
        _repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_EventoNoExiste_Falla()
    {
        // Arrange
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);
        _repo.Setup(r=>r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);

        // Act
        var res = await _handler.Handle(Cmd(_eventId), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("no encontrado");
        _repo.Verify(r=>r.ObtenerPorIdAsync(_eventId, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EventoYaPublicado_Falla()
    {
        // Arrange
        _repo.Setup(r=>r.ObtenerPorIdAsync(_eventoPublicado.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(_eventoPublicado);

        // Act
        var res = await _handler.Handle(Cmd(_eventoPublicado.Id), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("No se puede publicar");
    }
}
