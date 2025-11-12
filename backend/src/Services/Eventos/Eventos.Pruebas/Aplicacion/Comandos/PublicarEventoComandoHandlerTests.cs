using Eventos.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Moq;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class PublicarEventoComandoHandlerTests
{
    private readonly Mock<IRepositorioEvento> _eventRepositoryMock;
    private readonly PublicarEventoComandoHandler _handler;

    public PublicarEventoComandoHandlerTests()
    {
        _eventRepositoryMock = new Mock<IRepositorioEvento>();
        _handler = new PublicarEventoComandoHandler(_eventRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ConEventIdValido_DeberiaPublicarEvento()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var comando = new PublicarEventoComando(eventId);
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Ubicacion("Av Principal", "La California", "Caracas", "DF", "1089", "Venezuela");
        var eventEntity = new Evento("Evento Peliculas", "Mejores peliculas del 2025", direccion, startDate, endDate, 500, "organizador-001");
        typeof(Evento).GetProperty("Id")!.SetValue(eventEntity, eventId);

        _eventRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock
            .Setup(x => x.ActualizarAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.EsExitoso.Should().BeTrue();
        eventEntity.Estado.Should().Be(EstadoEvento.Publicado);
        _eventRepositoryMock.Verify(
            x => x.ActualizarAsync(It.Is<Evento>(e => e.Estado == EstadoEvento.Publicado), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConEventoInexistente_DeberiaRetornarFalla()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var comando = new PublicarEventoComando(eventId);

        _eventRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Evento?)null);

        // Act
        var result = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        result.EsExitoso.Should().BeFalse();
        result.Error.Should().Be("Evento no encontrado");
    }
}
