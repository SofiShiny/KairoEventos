/*using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumerados;
using Eventos.Dominio.ObjetosValor;
using Eventos.Infraestructura.Persistencia;
using Eventos.Infraestructura.Repositorios;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventos.Pruebas.Infraestructura;

public class EventoRepositoryTests
{
    private readonly EventosDbContext _context;
    private readonly EventoRepository _repository;

    public EventoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EventosDbContext>()
            .UseInMemoryDatabase(databaseNombre: Guid.NewGuid().ToString())
            .Options;

        _context = new EventosDbContext(options);
        _repository = new EventoRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEventoToDatabase()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "State", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "organizador-123");

        // Act
        var result = await _repository.AddAsync(eventEntity, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        var savedEvento = await _context.Eventos.FindAsync(result.Id);
        savedEvento.Should().NotBeNull();
        savedEvento!.Title.Should().Be("Tech Conference");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnEvento()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "State", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "organizador-123");
        await _context.Eventos.AddAsync(eventEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(eventEntity.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(eventEntity.Id);
        result.Title.Should().Be("Tech Conference");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEvento()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "State", "12345", "USA");
        var eventEntity = new Evento("Original Title", "Description", direccion, startDate, endDate, 500, "organizador-123");
        await _context.Eventos.AddAsync(eventEntity);
        await _context.SaveChangesAsync();

        // Act
        eventEntity.Publicar();
        await _repository.UpdateAsync(eventEntity, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var updatedEvento = await _context.Eventos.FindAsync(eventEntity.Id);
        updatedEvento.Should().NotBeNull();
        updatedEvento!.Status.Should().Be(EventoStatus.Publicado);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEvento()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "State", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "organizador-123");
        await _context.Eventos.AddAsync(eventEntity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(eventEntity.Id, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var deletedEvento = await _context.Eventos.FindAsync(eventEntity.Id);
        deletedEvento.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEventos()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "State", "12345", "USA");
        
        var event1 = new Evento("Conference 1", "Description 1", direccion, startDate, endDate, 100, "organizador-123");
        var event2 = new Evento("Conference 2", "Description 2", direccion, startDate.AddDays(10), endDate.AddDays(10), 200, "organizador-456");
        
        await _context.Eventos.AddRangeAsync(event1, event2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(e => e.Title == "Conference 1");
        results.Should().Contain(e => e.Title == "Conference 2");
    }
}
*/