using Comunidad.Domain.Entidades;
using Comunidad.Infrastructure.Persistencia;
using Comunidad.Infrastructure.Repositorios;
using FluentAssertions;
using Mongo2Go;
using Xunit;

namespace Comunidad.Tests.Infraestructura;

/// <summary>
/// Tests de integración para ComentarioRepository usando MongoDB en memoria
/// </summary>
public class ComentarioRepositoryIntegrationTests : IDisposable
{
    private readonly MongoDbRunner _mongoRunner;
    private readonly MongoDbContext _context;
    private readonly ComentarioRepository _repository;

    public ComentarioRepositoryIntegrationTests()
    {
        _mongoRunner = MongoDbRunner.Start();
        _context = new MongoDbContext(_mongoRunner.ConnectionString, "TestDb");
        _repository = new ComentarioRepository(_context);
    }

    [Fact]
    public async Task CrearAsync_ComentarioValido_InsertaEnMongoDB()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(foroId, usuarioId, "Contenido del comentario");

        // Act
        await _repository.CrearAsync(comentario);

        // Assert
        var comentarioGuardado = await _repository.ObtenerPorIdAsync(comentario.Id);
        comentarioGuardado.Should().NotBeNull();
        comentarioGuardado!.ForoId.Should().Be(foroId);
        comentarioGuardado.UsuarioId.Should().Be(usuarioId);
        comentarioGuardado.Contenido.Should().Be("Contenido del comentario");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ComentarioExiste_RetornaComentario()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(foroId, usuarioId, "Contenido esperado");
        await _repository.CrearAsync(comentario);

        // Act
        var resultado = await _repository.ObtenerPorIdAsync(comentario.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(comentario.Id);
        resultado.Contenido.Should().Be("Contenido esperado");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ComentarioNoExiste_RetornaNull()
    {
        // Arrange
        var comentarioIdInexistente = Guid.NewGuid();

        // Act
        var resultado = await _repository.ObtenerPorIdAsync(comentarioIdInexistente);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorForoIdAsync_ForoConComentarios_RetornaLista()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var comentario1 = Comentario.Crear(foroId, Guid.NewGuid(), "Comentario 1");
        var comentario2 = Comentario.Crear(foroId, Guid.NewGuid(), "Comentario 2");
        await _repository.CrearAsync(comentario1);
        await _repository.CrearAsync(comentario2);

        // Act
        var resultado = await _repository.ObtenerPorForoIdAsync(foroId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(c => c.ForoId.Should().Be(foroId));
    }

    [Fact]
    public async Task ObtenerPorForoIdAsync_ForoSinComentarios_RetornaListaVacia()
    {
        // Arrange
        var foroIdSinComentarios = Guid.NewGuid();

        // Act
        var resultado = await _repository.ObtenerPorForoIdAsync(foroIdSinComentarios);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ActualizarAsync_ComentarioValido_ActualizaEnMongoDB()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(foroId, usuarioId, "Contenido original");
        await _repository.CrearAsync(comentario);

        // Act
        comentario.Ocultar();
        await _repository.ActualizarAsync(comentario);

        // Assert
        var comentarioActualizado = await _repository.ObtenerPorIdAsync(comentario.Id);
        comentarioActualizado.Should().NotBeNull();
        comentarioActualizado!.EsVisible.Should().BeFalse();
    }

    [Fact]
    public async Task ActualizarAsync_ComentarioConRespuestas_ActualizaCorrectamente()
    {
        // Arrange
        var foroId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var comentario = Comentario.Crear(foroId, usuarioId, "Comentario principal");
        await _repository.CrearAsync(comentario);

        // Act
        comentario.AgregarRespuesta(Guid.NewGuid(), "Respuesta 1");
        await _repository.ActualizarAsync(comentario);

        // Assert
        var comentarioActualizado = await _repository.ObtenerPorIdAsync(comentario.Id);
        comentarioActualizado.Should().NotBeNull();
        comentarioActualizado!.Respuestas.Should().HaveCount(1);
        comentarioActualizado.Respuestas[0].Contenido.Should().Be("Respuesta 1");
    }

    [Fact]
    public async Task ObtenerPorForoIdAsync_MultiplesForos_RetornaSoloDelForoCorrecto()
    {
        // Arrange
        var foroId1 = Guid.NewGuid();
        var foroId2 = Guid.NewGuid();
        var comentarioForo1 = Comentario.Crear(foroId1, Guid.NewGuid(), "Comentario Foro 1");
        var comentarioForo2 = Comentario.Crear(foroId2, Guid.NewGuid(), "Comentario Foro 2");
        await _repository.CrearAsync(comentarioForo1);
        await _repository.CrearAsync(comentarioForo2);

        // Act
        var resultadoForo1 = await _repository.ObtenerPorForoIdAsync(foroId1);

        // Assert
        resultadoForo1.Should().HaveCount(1);
        resultadoForo1[0].ForoId.Should().Be(foroId1);
        resultadoForo1[0].Contenido.Should().Be("Comentario Foro 1");
    }

    public void Dispose()
    {
        _mongoRunner?.Dispose();
    }
}
