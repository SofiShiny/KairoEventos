using Comunidad.Application.Comandos;
using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;

namespace Comunidad.Tests.Aplicacion;

public class ResponderComentarioComandoHandlerTests
{
    private readonly Mock<IComentarioRepository> _comentarioRepositoryMock;
    private readonly ResponderComentarioComandoHandler _handler;

    public ResponderComentarioComandoHandlerTests()
    {
        _comentarioRepositoryMock = new Mock<IComentarioRepository>();
        _handler = new ResponderComentarioComandoHandler(_comentarioRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComentarioExiste_AgregaRespuestaYActualiza()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var contenido = "Esta es una respuesta";
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario original");

        var comando = new ResponderComentarioComando(comentarioId, usuarioId, contenido);

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
        comentario.Respuestas.Should().HaveCount(1);
        comentario.Respuestas[0].UsuarioId.Should().Be(usuarioId);
        comentario.Respuestas[0].Contenido.Should().Be(contenido);
        _comentarioRepositoryMock.Verify(x => x.ActualizarAsync(comentario), Times.Once);
    }

    [Fact]
    public async Task Handle_ComentarioNoExiste_LanzaInvalidOperationException()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comando = new ResponderComentarioComando(comentarioId, Guid.NewGuid(), "Respuesta");

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
    public async Task Handle_ContenidoVacio_AgregaRespuestaConContenidoVacio()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario original");

        var comando = new ResponderComentarioComando(comentarioId, Guid.NewGuid(), "");

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
        comentario.Respuestas.Should().HaveCount(1);
        comentario.Respuestas[0].Contenido.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RepositorioFalla_PropagaExcepcion()
    {
        // Arrange
        var comentarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(Guid.NewGuid(), Guid.NewGuid(), "Comentario");

        var comando = new ResponderComentarioComando(comentarioId, Guid.NewGuid(), "Respuesta");

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
