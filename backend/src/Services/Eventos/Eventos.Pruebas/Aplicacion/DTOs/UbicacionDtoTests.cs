/*using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class LocationDtoTests
{
    [Fact]
    public void LocationDto_ShouldInitializeAllProperties()
    {
        // Arrange & Act
        var dto = new LocationDto
        {
            VenueNombre = "Conference Center",
            Direccion = "123 Main Street",
            Ciudad = "San Francisco",
            State = "CA",
            ZipCode = "94105"
        };

        // Assert
        dto.VenueNombre.Should().Be("Conference Center");
        dto.Direccion.Should().Be("123 Main Street");
        dto.Ciudad.Should().Be("San Francisco");
        dto.State.Should().Be("CA");
        dto.ZipCode.Should().Be("94105");
    }

    [Fact]
    public void LocationDto_ShouldAllowEmptyStrings()
    {
        // Arrange & Act
        var dto = new LocationDto
        {
            VenueNombre = string.Empty,
            Direccion = string.Empty,
            Ciudad = string.Empty,
            State = string.Empty,
            ZipCode = string.Empty
        };

        // Assert
        dto.VenueNombre.Should().BeEmpty();
        dto.Direccion.Should().BeEmpty();
        dto.Ciudad.Should().BeEmpty();
        dto.State.Should().BeEmpty();
        dto.ZipCode.Should().BeEmpty();
    }
}
*/