using Comunidad.Application.Comandos;
using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;

namespace Comunidad.Tests.Aplicacion;

public class OcultarComentarioComandoHandlerTests
{
    private readonly Mock<IComentarioRepository> _comentarioRepositoryMock;
    private readonly OcultarComentarioComandoHandler _handler;

    public OcultarComentarioComandoHandlerTests()
    {
        _comentarioRepositoryMock = new Mock<IComentarioRepository>();
        _handler = new OcultarComentarioComandoHandler(_comentarioRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComentarioExiste_OcultaComentarioYActualiza()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario a ocultar");

        var comando = new OcultarComentarioComando(comentarioId);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comentarioId))
            .ReturnsAsync(comentario);

        _comentarioRepositoryMock
            .Setup(x => x.ActualizarAsync(It.IsAny<Comentario>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().Be(MediatR.Unit.Value);
        comentario.EsVisible.Should().BeFalse();
        _comentarioRepositoryMock.Verify(x => x.ActualizarAsync(comentario), Times.Once);
    }

    [Fact]
    public async Task Handle_ComentarioNoExiste_LanzaInvalidOperationException()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comando = new OcultarComentarioComando(comentarioId);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comentarioId))
            .ReturnsAsync((Comentario?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"El comentario con ID {comentarioId} no existe");
        _comentarioRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comentario>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComentarioYaOculto_MantieneFalso()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");
        comentario.Ocultar(); // Ya está oculto

        var comando = new OcultarComentarioComando(comentarioId);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comentarioId))
            .ReturnsAsync(comentario);

        _comentarioRepositoryMock
            .Setup(x => x.ActualizarAsync(It.IsAny<Comentario>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().Be(MediatR.Unit.Value);
        comentario.EsVisible.Should().BeFalse();
        _comentarioRepositoryMock.Verify(x => x.ActualizarAsync(comentario), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositorioFalla_PropagaExcepcion()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");

        var comando = new OcultarComentarioComando(comentarioId);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comentarioId))
            .ReturnsAsync(comentario);

        _comentarioRepositoryMock
            .Setup(x => x.ActualizarAsync(It.IsAny<Comentario>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Error de base de datos");
    }
}
