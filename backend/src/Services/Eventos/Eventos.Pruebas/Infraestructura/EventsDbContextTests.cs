/*using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosValor;
using Eventos.Infraestructura.Persistencia;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventos.Pruebas.Infraestructura;

public class EventosDbContextTests
{
    private EventosDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EventosDbContext>()
            .UseInMemoryDatabase(databaseNombre: Guid.NewGuid().ToString())
            .Options;

        return new EventosDbContext(options);
    }

    [Fact]
    public void EventosDbContext_ShouldHaveEventosDbSet()
    {
        // Arrange
        using var context = CreateDbContext();

        // Act & Assert
        context.Eventos.Should().NotBeNull();
    }

    [Fact]
    public async Task EventosDbContext_ShouldSaveEvento()
    {
        // Arrange
        using var context = CreateDbContext();
        var direccion = new Location("Prueba Venue", "123 Main St", "Ciudad", "State", "12345", "USA");
        var @evento = new Evento(
            "Prueba Evento", 
            "Description",
            direccion,
            DateTime.UtcNow.AddDays(30), 
            DateTime.UtcNow.AddDays(30).AddHours(4), 
            100,
            "organizador-123");

        // Act
        context.Eventos.Add(@evento);
        await context.SaveChangesAsync();

        // Assert
        var savedEvento = await context.Eventos.FindAsync(@evento.Id);
        savedEvento.Should().NotBeNull();
        savedEvento!.Title.Should().Be("Prueba Evento");
    }

    [Fact]
    public async Task EventosDbContext_ShouldTrackChanges()
    {
        // Arrange
        using var context = CreateDbContext();
        var direccion = new Location("Prueba Venue", "123 Main St", "Ciudad", "State", "12345", "USA");
        var @evento = new Evento(
            "Original Nombre", 
            "Description",
            direccion,
            DateTime.UtcNow.AddDays(30), 
            DateTime.UtcNow.AddDays(30).AddHours(4), 
            100,
            "organizador-123");

        context.Eventos.Add(@evento);
        await context.SaveChangesAsync();

        // Act
        @evento.Update("Updated Nombre", "New Description", direccion, @evento.StartDate, @evento.EndDate, 100);
        await context.SaveChangesAsync();

        // Assert
        var updatedEvento = await context.Eventos.FindAsync(@evento.Id);
        updatedEvento!.Title.Should().Be("Updated Nombre");
        updatedEvento.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task EventosDbContext_ShouldDeleteEvento()
    {
        // Arrange
        using var context = CreateDbContext();
        var direccion = new Location("Prueba Venue", "123 Main St", "Ciudad", "State", "12345", "USA");
        var @evento = new Evento(
            "Evento to Delete", 
            "Description",
            direccion,
            DateTime.UtcNow.AddDays(30), 
            DateTime.UtcNow.AddDays(30).AddHours(4), 
            100,
            "organizador-123");

        context.Eventos.Add(@evento);
        await context.SaveChangesAsync();

        // Act
        context.Eventos.Remove(@evento);
        await context.SaveChangesAsync();

        // Assert
        var deletedEvento = await context.Eventos.FindAsync(@evento.Id);
        deletedEvento.Should().BeNull();
    }
}
*/