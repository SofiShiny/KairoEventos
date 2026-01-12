using Comunidad.Domain.Entidades;
using Comunidad.Infrastructure.Persistencia;
using Comunidad.Infrastructure.Repositorios;
using FluentAssertions;
using Mongo2Go;
using Xunit;

namespace Comunidad.Tests.Infraestructura;

/// <summary>
/// Tests de integración para ForoRepository usando MongoDB en memoria
/// </summary>
public class ForoRepositoryIntegrationTests : IDisposable
{
    private readonly MongoDbRunner _mongoRunner;
    private readonly MongoDbContext _context;
    private readonly ForoRepository _repository;

    public ForoRepositoryIntegrationTests()
    {
        _mongoRunner = MongoDbRunner.Start();
        _context = new MongoDbContext(_mongoRunner.ConnectionString, "TestDb");
        _repository = new ForoRepository(_context);
    }

    [Fact]
    public async Task CrearAsync_ForoValido_InsertaEnMongoDB()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var foro = Foro.Crear(eventoId, "Foro de Prueba");

        // Act
        await _repository.CrearAsync(foro);

        // Assert
        var foroGuardado = await _repository.ObtenerPorEventoIdAsync(eventoId);
        foroGuardado.Should().NotBeNull();
        foroGuardado!.EventoId.Should().Be(eventoId);
        foroGuardado.Titulo.Should().Be("Foro de Prueba");
    }

    [Fact]
    public async Task ObtenerPorEventoIdAsync_ForoExiste_RetornaForo()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var foro = Foro.Crear(eventoId, "Foro Existente");
        await _repository.CrearAsync(foro);

        // Act
        var resultado = await _repository.ObtenerPorEventoIdAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        resultado.Titulo.Should().Be("Foro Existente");
    }

    [Fact]
    public async Task ObtenerPorEventoIdAsync_ForoNoExiste_RetornaNull()
    {
        // Arrange
        var eventoIdInexistente = Guid.NewGuid();

        // Act
        var resultado = await _repository.ObtenerPorEventoIdAsync(eventoIdInexistente);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExistePorEventoIdAsync_ForoExiste_RetornaTrue()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var foro = Foro.Crear(eventoId, "Foro para Verificar");
        await _repository.CrearAsync(foro);

        // Act
        var existe = await _repository.ExistePorEventoIdAsync(eventoId);

        // Assert
        existe.Should().BeTrue();
    }

    [Fact]
    public async Task ExistePorEventoIdAsync_ForoNoExiste_RetornaFalse()
    {
        // Arrange
        var eventoIdInexistente = Guid.NewGuid();

        // Act
        var existe = await _repository.ExistePorEventoIdAsync(eventoIdInexistente);

        // Assert
        existe.Should().BeFalse();
    }

    [Fact]
    public async Task CrearAsync_MultiplesForos_TodosSeGuardan()
    {
        // Arrange
        var eventoId1 = Guid.NewGuid();
        var eventoId2 = Guid.NewGuid();
        var foro1 = Foro.Crear(eventoId1, "Foro 1");
        var foro2 = Foro.Crear(eventoId2, "Foro 2");

        // Act
        await _repository.CrearAsync(foro1);
        await _repository.CrearAsync(foro2);

        // Assert
        var existe1 = await _repository.ExistePorEventoIdAsync(eventoId1);
        var existe2 = await _repository.ExistePorEventoIdAsync(eventoId2);
        existe1.Should().BeTrue();
        existe2.Should().BeTrue();
    }

    public void Dispose()
    {
        _mongoRunner?.Dispose();
    }
}
