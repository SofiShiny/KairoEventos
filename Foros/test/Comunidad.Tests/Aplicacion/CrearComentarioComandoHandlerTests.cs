using Comunidad.Application.Comandos;
using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;

namespace Comunidad.Tests.Aplicacion;

public class CrearComentarioComandoHandlerTests
{
    private readonly Mock<IComentarioRepository> _comentarioRepositoryMock;
    private readonly Mock<IForoRepository> _foroRepositoryMock;
    private readonly CrearComentarioComandoHandler _handler;

    public CrearComentarioComandoHandlerTests()
    {
        _comentarioRepositoryMock = new Mock<IComentarioRepository>();
        _foroRepositoryMock = new Mock<IForoRepository>();
        _handler = new CrearComentarioComandoHandler(
            _comentarioRepositoryMock.Object,
            _foroRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForoExiste_CreaComentarioYRetornaId()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var contenido = "Este es un comentario de prueba";
        var foro = Foro.Crear(Guid.NewGuid(), "Foro de prueba");

        var comando = new CrearComentarioComando(foroId, usuarioId, contenido);

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(foroId))
            .ReturnsAsync(foro);

        _comentarioRepositoryMock
            .Setup(x => x.CrearAsync(It.IsAny<Comentario>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().NotBeEmpty();
        _foroRepositoryMock.Verify(x => x.ObtenerPorEventoIdAsync(foroId), Times.Once);
        _comentarioRepositoryMock.Verify(x => x.CrearAsync(It.Is<Comentario>(c =>
            c.ForoId == foroId &&
            c.UsuarioId == usuarioId &&
            c.Contenido == contenido &&
            c.EsVisible == true
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_ForoNoExiste_LanzaInvalidOperationException()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var comando = new CrearComentarioComando(foroId, Guid.NewGuid(), "Contenido");

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(foroId))
            .ReturnsAsync((Foro?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"El foro con ID {foroId} no existe");
        _comentarioRepositoryMock.Verify(x => x.CrearAsync(It.IsAny<Comentario>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ContenidoVacio_CreaComentarioConContenidoVacio()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var foro = Foro.Crear(Guid.NewGuid(), "Foro de prueba");

        var comando = new CrearComentarioComando(foroId, Guid.NewGuid(), "");

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(foroId))
            .ReturnsAsync(foro);

        _comentarioRepositoryMock
            .Setup(x => x.CrearAsync(It.IsAny<Comentario>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _handler.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().NotBeEmpty();
        _comentarioRepositoryMock.Verify(x => x.CrearAsync(It.Is<Comentario>(c =>
            c.Contenido == ""
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositorioFalla_PropagaExcepcion()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var foro = Foro.Crear(Guid.NewGuid(), "Foro de prueba");

        var comando = new CrearComentarioComando(foroId, Guid.NewGuid(), "Contenido");

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(foroId))
            .ReturnsAsync(foro);

        _comentarioRepositoryMock
            .Setup(x => x.CrearAsync(It.IsAny<Comentario>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Error de base de datos");
    }
}
