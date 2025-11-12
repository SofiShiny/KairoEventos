/*using Eventos.Dominio.ObjetosValor;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class LocationTests
{
    [Fact]
    public void CreateLocation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var venueNombre = "Convention Center";
        var address = "123 Main St";
        var ciudad = "New York";
        var state = "NY";
        var codigoPostal = "10001";
        var pais = "USA";

        // Act
        var direccion = new Location(venueNombre, address, ciudad, state, codigoPostal, pais);

        // Assert
        direccion.Should().NotBeNull();
        direccion.VenueNombre.Should().Be(venueNombre);
        direccion.Direccion.Should().Be(address);
        direccion.Ciudad.Should().Be(ciudad);
        direccion.State.Should().Be(state);
        direccion.CodigoPostal.Should().Be(codigoPostal);
        direccion.Pais.Should().Be(pais);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateLocation_WithInvalidVenueNombre_ShouldThrowException(string venueNombre)
    {
        // Act
        Action act = () => new Location(venueNombre, "Direccion", "Ciudad", "State", "12345", "Pais");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*VenueNombre*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateLocation_WithInvalidCity_ShouldThrowException(string ciudad)
    {
        // Act
        Action act = () => new Location("Venue", "Direccion", ciudad, "State", "12345", "Pais");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Ciudad*");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var location1 = new Location("Venue", "123 Main St", "New York", "NY", "10001", "USA");
        var location2 = new Location("Venue", "123 Main St", "New York", "NY", "10001", "USA");

        // Act
        var areEqual = location1.Equals(location2);

        // Assert
        areEqual.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var location1 = new Location("Venue 1", "123 Main St", "New York", "NY", "10001", "USA");
        var location2 = new Location("Venue 2", "456 Oak Ave", "Boston", "MA", "02101", "USA");

        // Act
        var areEqual = location1.Equals(location2);

        // Assert
        areEqual.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var location1 = new Location("Venue", "123 Main St", "New York", "NY", "10001", "USA");
        var location2 = new Location("Venue", "123 Main St", "New York", "NY", "10001", "USA");

        // Act
        var hash1 = location1.GetHashCode();
        var hash2 = location2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }
}
*/