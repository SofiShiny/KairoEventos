using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.EventosDeDominio;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using MassTransit;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class CancelarEventoComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _repo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Mock<ILogger<CancelarEventoComandoHandler>> _logger;
    private readonly CancelarEventoComandoHandler _handler;
    private readonly Guid _eventId;
    private readonly Evento _evento;

    public CancelarEventoComandoHandlerTests()
    {
        _repo = new Mock<IRepositorioEvento>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        _logger = new Mock<ILogger<CancelarEventoComandoHandler>>();
        _handler = new CancelarEventoComandoHandler(_repo.Object, _publishEndpoint.Object, _logger.Object);
        
        _eventId = Guid.NewGuid();
        var ubicacion = new Ubicacion("Lugar", "Dir", "Ciudad", "Reg", "0000", "Pais");
        // Asegurar que la fecha sea siempre futura en el constructor
        _evento = new Evento("Titulo", "Desc", ubicacion, DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), 10, "org-1");
        typeof(Evento).GetProperty("Id")!.SetValue(_evento, _eventId);
    }

    [Fact]
    public async Task Handle_EventoExisteYEstaPublicado_CancelaEvento()
    {
        // Arrange
        _evento.Publicar();
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, false, It.IsAny<CancellationToken>())).ReturnsAsync(_evento);
        _repo.Setup(r => r.ActualizarAsync(_evento, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var res = await _handler.Handle(new CancelarEventoComando(_eventId), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeTrue();
        _evento.Estado.Should().Be(EstadoEvento.Cancelado);
        // Verificar usando el tipo específico del evento de dominio
        _publishEndpoint.Verify(p => p.Publish(It.IsAny<EventoCanceladoEventoDominio>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EventoNoExiste_Falla()
    {
        // Arrange
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, false, It.IsAny<CancellationToken>())).ReturnsAsync((Evento?)null);

        // Act
        var res = await _handler.Handle(new CancelarEventoComando(_eventId), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task Handle_EventoYaCancelado_Falla()
    {
        // Arrange
        _evento.Publicar();
        _evento.Cancelar();
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, false, It.IsAny<CancellationToken>())).ReturnsAsync(_evento);

        // Act
        var res = await _handler.Handle(new CancelarEventoComando(_eventId), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_Falla()
    {
        // Arrange
        _repo.Setup(r => r.ObtenerPorIdAsync(_eventId, false, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Error fatal"));

        // Act
        var res = await _handler.Handle(new CancelarEventoComando(_eventId), CancellationToken.None);

        // Assert
        res.EsExitoso.Should().BeFalse();
        res.Error.Should().Contain("Error inesperado");
    }
}
