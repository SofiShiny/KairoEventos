/*using Eventos.Dominio.EventosDominio;
using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;
using System;

namespace Eventos.Pruebas.Dominio.EventosDominio;

public class EventoPublishedDomainEventoTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenCalled()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventNombre = "Tech Conference 2024";
        var publishedAt = DateTime.UtcNow;

        // Act
        var domainEvento = new EventoPublishedDomainEvento(eventId, eventNombre, publishedAt);

        // Assert
        domainEvento.EventoId.Should().Be(eventId);
        domainEvento.EventoNombre.Should().Be(eventNombre);
        domainEvento.PublishedAt.Should().Be(publishedAt);
        domainEvento.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EventoPublishedDomainEvento_ShouldBeOfTypeDomainEvento()
    {
        // Arrange & Act
        var domainEvento = new EventoPublishedDomainEvento(Guid.NewGuid(), "Prueba", DateTime.UtcNow);

        // Assert
        domainEvento.Should().BeAssignableTo<IDomainEvento>();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Constructor_ShouldAcceptEmptyGuid_WhenProvided(string guidString)
    {
        // Arrange
        var eventId = Guid.Parse(guidString);

        // Act
        var domainEvento = new EventoPublishedDomainEvento(eventId, "Prueba", DateTime.UtcNow);

        // Assert
        domainEvento.EventoId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullOrEmptyNombre_WhenProvided()
    {
        // Arrange & Act
        var domainEvento1 = new EventoPublishedDomainEvento(Guid.NewGuid(), null!, DateTime.UtcNow);
        var domainEvento2 = new EventoPublishedDomainEvento(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        // Assert
        domainEvento1.EventoNombre.Should().BeNull();
        domainEvento2.EventoNombre.Should().BeEmpty();
    }

    [Fact]
    public void PublishedAt_ShouldAcceptPastDates()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var domainEvento = new EventoPublishedDomainEvento(Guid.NewGuid(), "Prueba", pastDate);

        // Assert
        domainEvento.PublishedAt.Should().Be(pastDate);
    }

    [Fact]
    public void PublishedAt_ShouldAcceptFutureDates()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(10);

        // Act
        var domainEvento = new EventoPublishedDomainEvento(Guid.NewGuid(), "Prueba", futureDate);

        // Assert
        domainEvento.PublishedAt.Should().Be(futureDate);
    }
}
*/