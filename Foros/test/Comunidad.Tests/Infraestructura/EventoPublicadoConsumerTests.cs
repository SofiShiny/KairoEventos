using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using Comunidad.Infrastructure.Consumers;
using Eventos.Domain.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Comunidad.Tests.Infraestructura;

public class EventoPublicadoConsumerTests
{
    private readonly Mock<IForoRepository> _foroRepositoryMock;
    private readonly Mock<ILogger<EventoPublicadoConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<EventoPublicadoEventoDominio>> _contextMock;
    private readonly EventoPublicadoConsumer _consumer;

    public EventoPublicadoConsumerTests()
    {
        _foroRepositoryMock = new Mock<IForoRepository>();
        _loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();
        _contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        _consumer = new EventoPublicadoConsumer(_foroRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_EventoNuevo_CreaForoEnMongoDB()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Conferencia Tech 2024",
            FechaInicio = DateTime.UtcNow.AddDays(30)
        };

        _contextMock.Setup(x => x.Message).Returns(evento);

        _foroRepositoryMock
            .Setup(x => x.ExistePorEventoIdAsync(eventoId))
            .ReturnsAsync(false);

        _foroRepositoryMock
            .Setup(x => x.CrearAsync(It.IsAny<Foro>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _foroRepositoryMock.Verify(x => x.ExistePorEventoIdAsync(eventoId), Times.Once);
        _foroRepositoryMock.Verify(x => x.CrearAsync(It.Is<Foro>(f =>
            f.EventoId == eventoId &&
            f.Titulo == "Conferencia Tech 2024"
        )), Times.Once);
    }

    [Fact]
    public async Task Consume_ForoYaExiste_NoCreaDuplicado()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Evento Duplicado",
            FechaInicio = DateTime.UtcNow
        };

        _contextMock.Setup(x => x.Message).Returns(evento);

        _foroRepositoryMock
            .Setup(x => x.ExistePorEventoIdAsync(eventoId))
            .ReturnsAsync(true);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _foroRepositoryMock.Verify(x => x.ExistePorEventoIdAsync(eventoId), Times.Once);
        _foroRepositoryMock.Verify(x => x.CrearAsync(It.IsAny<Foro>()), Times.Never);
    }

    [Fact]
    public async Task Consume_RepositorioFalla_LanzaExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "Evento con Error",
            FechaInicio = DateTime.UtcNow
        };

        _contextMock.Setup(x => x.Message).Returns(evento);

        _foroRepositoryMock
            .Setup(x => x.ExistePorEventoIdAsync(eventoId))
            .ReturnsAsync(false);

        _foroRepositoryMock
            .Setup(x => x.CrearAsync(It.IsAny<Foro>()))
            .ThrowsAsync(new Exception("Error de MongoDB"));

        // Act
        Func<Task> act = async () => await _consumer.Consume(_contextMock.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Error de MongoDB");
    }

    [Fact]
    public async Task Consume_EventoConTituloVacio_CreaForoConTituloVacio()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var evento = new EventoPublicadoEventoDominio
        {
            EventoId = eventoId,
            TituloEvento = "",
            FechaInicio = DateTime.UtcNow
        };

        _contextMock.Setup(x => x.Message).Returns(evento);

        _foroRepositoryMock
            .Setup(x => x.ExistePorEventoIdAsync(eventoId))
            .ReturnsAsync(false);

        _foroRepositoryMock
            .Setup(x => x.CrearAsync(It.IsAny<Foro>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _foroRepositoryMock.Verify(x => x.CrearAsync(It.Is<Foro>(f =>
            f.Titulo == ""
        )), Times.Once);
    }
}
