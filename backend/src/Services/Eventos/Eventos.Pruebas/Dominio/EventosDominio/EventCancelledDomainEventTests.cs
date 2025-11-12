/*using Eventos.Dominio.EventosDominio;
using FluentAssertions;
using Xunit;
using System;
using BloquesConstruccion.Dominio;

namespace Eventos.Pruebas.Dominio.EventosDominio;

public class EventoCancelledDomainEventoTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenCalled()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var reason = "Venue unavailable";
        var cancelledAt = DateTime.UtcNow;

        // Act
        var domainEvento = new EventoCancelledDomainEvento(eventId, reason, cancelledAt);

        // Assert
        domainEvento.EventoId.Should().Be(eventId);
        domainEvento.Reason.Should().Be(reason);
        domainEvento.CancelledAt.Should().Be(cancelledAt);
        domainEvento.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EventoCancelledDomainEvento_ShouldImplementIDomainEvento()
    {
        // Arrange & Act
        var domainEvento = new EventoCancelledDomainEvento(Guid.NewGuid(), "Prueba", DateTime.UtcNow);

        // Assert
        domainEvento.Should().BeAssignableTo<IDomainEvento>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldAcceptEmptyOrNullReason(string? reason)
    {
        // Arrange & Act
        var domainEvento = new EventoCancelledDomainEvento(Guid.NewGuid(), reason!, DateTime.UtcNow);

        // Assert
        domainEvento.Reason.Should().Be(reason);
    }

    [Fact]
    public void Constructor_ShouldAcceptEmptyGuid()
    {
        // Arrange & Act
        var domainEvento = new EventoCancelledDomainEvento(Guid.Empty, "Cancelled", DateTime.UtcNow);

        // Assert
        domainEvento.EventoId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void CancelledAt_ShouldAcceptDifferentTimeValues()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-5);
        var futureDate = DateTime.UtcNow.AddDays(5);

        // Act
        var pastEvento = new EventoCancelledDomainEvento(Guid.NewGuid(), "Prueba", pastDate);
        var futureEvento = new EventoCancelledDomainEvento(Guid.NewGuid(), "Prueba", futureDate);

        // Assert
        pastEvento.CancelledAt.Should().Be(pastDate);
        futureEvento.CancelledAt.Should().Be(futureDate);
    }
}
*/