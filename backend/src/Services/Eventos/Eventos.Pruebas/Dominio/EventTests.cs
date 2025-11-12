/*using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumerados;
using Eventos.Dominio.ObjetosValor;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class EventoTests
{
    [Fact]
    public void CreateEvento_WithValidData_ShouldSucceed()
    {
        // Arrange
        var title = "Tech Conference 2025";
        var description = "Annual technology conference";
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Convention Center", "123 Main St", "New York", "NY", "10001", "USA");
        var maxAsistentes = 500;
        var organizadorId = "organizador-123";

        // Act
        var eventEntity = new Evento(title, description, direccion, startDate, endDate, maxAsistentes, organizadorId);

        // Assert
        eventEntity.Should().NotBeNull();
        eventEntity.Title.Should().Be(title);
        eventEntity.Description.Should().Be(description);
        eventEntity.StartDate.Should().Be(startDate);
        eventEntity.EndDate.Should().Be(endDate);
        eventEntity.Location.Should().Be(direccion);
        eventEntity.MaxAsistentes.Should().Be(maxAsistentes);
        eventEntity.Status.Should().Be(EventoStatus.Draft);
        eventEntity.Asistentes.Should().BeEmpty();
    }

    [Fact]
    public void CreateEvento_WithEndDateBeforeStartDate_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(-1);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");

        // Act
        Action act = () => new Evento("Title", "Description", direccion, startDate, endDate, 100, "org-123");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("End date must be after start date*");
    }

    [Fact]
    public void CreateEvento_WithNegativeMaxAsistentes_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");

        // Act
        Action act = () => new Evento("Title", "Description", direccion, startDate, endDate, -1, "org-123");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Max asistentes must be greater than zero*");
    }

    [Fact]
    public void Publish_DraftEvento_ShouldChangeStatusToPublished()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "org-123");

        // Act
        eventEntity.Publicar();

        // Assert
        eventEntity.Status.Should().Be(EventoStatus.Publicado);
    }

    [Fact]
    public void Publish_AlreadyPublishedEvento_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "org-123");
        eventEntity.Publicar();

        // Act
        Action act = () => eventEntity.Publicar();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot publicar evento in Publicado status");
    }

    [Fact]
    public void Cancel_PublishedEvento_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "org-123");
        eventEntity.Publicar();

        // Act
        eventEntity.Cancel();

        // Assert
        eventEntity.Status.Should().Be(EventoStatus.Cancelled);
    }

    [Fact]
    public void RegistrarAsistente_WithAvailableCapacity_ShouldSucceed()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "org-123");
        eventEntity.Publicar();
        var userId = "user-123";
        var nombreUsuario = "John Doe";
        var email = "user@example.com";

        // Act
        eventEntity.RegistrarAsistente(userId, nombreUsuario, email);

        // Assert
        eventEntity.Asistentes.Should().ContainSingle();
        var asistente = eventEntity.Asistentes.First();
        asistente.UserId.Should().Be(userId);
        asistente.NombreUsuario.Should().Be(nombreUsuario);
        asistente.Email.Should().Be(email);
    }

    [Fact]
    public void RegistrarAsistente_WhenEventoNotPublished_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "org-123");
        var userId = "user-123";

        // Act
        Action act = () => eventEntity.RegistrarAsistente(userId, "John Doe", "user@example.com");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot register for an unpublished evento");
    }

    [Fact]
    public void RegistrarAsistente_WhenEventoFull_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 1, "org-123");
        eventEntity.Publicar();
        eventEntity.RegistrarAsistente("user-1", "User One", "user1@example.com");

        // Act
        Action act = () => eventEntity.RegistrarAsistente("user-2", "User Two", "user2@example.com");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Evento is full");
    }

    [Fact]
    public void IsFull_WhenAtMaxCapacity_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 2, "org-123");
        eventEntity.Publicar();
        eventEntity.RegistrarAsistente("user-1", "User One", "user1@example.com");
        eventEntity.RegistrarAsistente("user-2", "User Two", "user2@example.com");

        // Act
        var isFull = eventEntity.IsFull;

        // Assert
        isFull.Should().BeTrue();
    }

    [Fact]
    public void IsFull_WhenBelowMaxCapacity_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddDays(2);
        var direccion = new Location("Venue", "Direccion", "Ciudad", "ST", "12345", "USA");
        var eventEntity = new Evento("Tech Conference", "Description", direccion, startDate, endDate, 500, "org-123");
        eventEntity.Publicar();
        eventEntity.RegistrarAsistente("user-1", "User One", "user1@example.com");

        // Act
        var isFull = eventEntity.IsFull;

        // Assert
        isFull.Should().BeFalse();
    }
}
*/