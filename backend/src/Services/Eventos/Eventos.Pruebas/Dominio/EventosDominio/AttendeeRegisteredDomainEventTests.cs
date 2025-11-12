/*using Eventos.Dominio.EventosDominio;
using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;
using System;

namespace Eventos.Pruebas.Dominio.EventosDominio;

public class AsistenteRegistraredDomainEventoTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties_WhenCalled()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var asistenteId = Guid.NewGuid();
        var asistenteNombre = "John Doe";
        var asistenteEmail = "john@example.com";

        // Act
        var domainEvento = new AsistenteRegistraredDomainEvento(eventId, asistenteId, asistenteNombre, asistenteEmail);

        // Assert
        domainEvento.EventoId.Should().Be(eventId);
        domainEvento.AsistenteId.Should().Be(asistenteId);
        domainEvento.AsistenteNombre.Should().Be(asistenteNombre);
        domainEvento.AsistenteEmail.Should().Be(asistenteEmail);
        domainEvento.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AsistenteRegistraredDomainEvento_ShouldImplementIDomainEvento()
    {
        // Arrange & Act
        var domainEvento = new AsistenteRegistraredDomainEvento(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Prueba", 
            "prueba@prueba.com");

        // Assert
        domainEvento.Should().BeAssignableTo<IDomainEvento>();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("ValidNombre", "")]
    [InlineData("", "valid@email.com")]
    public void Constructor_ShouldAcceptEmptyOrNullStrings(string? name, string? email)
    {
        // Arrange & Act
        var domainEvento = new AsistenteRegistraredDomainEvento(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            name!, 
            email!);

        // Assert
        domainEvento.AsistenteNombre.Should().Be(name);
        domainEvento.AsistenteEmail.Should().Be(email);
    }

    [Fact]
    public void Constructor_ShouldAcceptEmptyGuids()
    {
        // Arrange & Act
        var domainEvento = new AsistenteRegistraredDomainEvento(
            Guid.Empty, 
            Guid.Empty, 
            "Prueba", 
            "prueba@prueba.com");

        // Assert
        domainEvento.EventoId.Should().Be(Guid.Empty);
        domainEvento.AsistenteId.Should().Be(Guid.Empty);
    }
}
*/