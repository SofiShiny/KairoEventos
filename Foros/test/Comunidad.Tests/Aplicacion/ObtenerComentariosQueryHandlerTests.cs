using Comunidad.Application.Consultas;
using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using FluentAssertions;
using Moq;
using Xunit;

namespace Comunidad.Tests.Aplicacion;

public class ObtenerComentariosQueryHandlerTests
{
    private readonly Mock<IComentarioRepository> _comentarioRepositoryMock;
    private readonly Mock<IForoRepository> _foroRepositoryMock;
    private readonly ObtenerComentariosQueryHandler _handler;

    public ObtenerComentariosQueryHandlerTests()
    {
        _comentarioRepositoryMock = new Mock<IComentarioRepository>();
        _foroRepositoryMock = new Mock<IForoRepository>();
        _handler = new ObtenerComentariosQueryHandler(
            _comentarioRepositoryMock.Object,
            _foroRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForoExisteConComentariosVisibles_RetornaComentariosVisibles()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var foroId = Guid.NewGuid();
        var foro = Foro.Crear(eventoId, "Foro de prueba");
        foro.Id = foroId;

        var comentario1 = Comentario.Crear(foroId, Guid.NewGuid(), "Comentario visible 1");
        var comentario2 = Comentario.Crear(foroId, Guid.NewGuid(), "Comentario visible 2");
        comentario2.AgregarRespuesta(Guid.NewGuid(), "Respuesta 1");

        var query = new ObtenerComentariosQuery(eventoId);

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync(foro);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorForoIdAsync(foroId))
            .ReturnsAsync(new List<Comentario> { comentario1, comentario2 });

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Contenido.Should().Be("Comentario visible 1");
        resultado[1].Contenido.Should().Be("Comentario visible 2");
        resultado[1].Respuestas.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ForoNoExiste_RetornaListaVacia()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var query = new ObtenerComentariosQuery(eventoId);

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync((Foro?)null);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeEmpty();
        _comentarioRepositoryMock.Verify(x => x.ObtenerPorForoIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComentariosOcultos_NoLosRetorna()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var foroId = Guid.NewGuid();
        var foro = Foro.Crear(eventoId, "Foro de prueba");
        foro.Id = foroId;

        var comentarioVisible = Comentario.Crear(foroId, Guid.NewGuid(), "Visible");
        var comentarioOculto = Comentario.Crear(foroId, Guid.NewGuid(), "Oculto");
        comentarioOculto.Ocultar();

        var query = new ObtenerComentariosQuery(eventoId);

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync(foro);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorForoIdAsync(foroId))
            .ReturnsAsync(new List<Comentario> { comentarioVisible, comentarioOculto });

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].Contenido.Should().Be("Visible");
    }

    [Fact]
    public async Task Handle_ForoSinComentarios_RetornaListaVacia()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var foroId = Guid.NewGuid();
        var foro = Foro.Crear(eventoId, "Foro vacío");
        foro.Id = foroId;

        var query = new ObtenerComentariosQuery(eventoId);

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ReturnsAsync(foro);

        _comentarioRepositoryMock
            .Setup(x => x.ObtenerPorForoIdAsync(foroId))
            .ReturnsAsync(new List<Comentario>());

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RepositorioFalla_PropagaExcepcion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var query = new ObtenerComentariosQuery(eventoId);

        _foroRepositoryMock
            .Setup(x => x.ObtenerPorEventoIdAsync(eventoId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Error de base de datos");
    }
}
