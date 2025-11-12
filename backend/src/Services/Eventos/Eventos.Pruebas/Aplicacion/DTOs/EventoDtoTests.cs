/*using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoDtoTests
{
    [Fact]
    public void EventoDto_ShouldInitializeAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Tech Conference";
        var description = "Annual evento";
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = startDate.AddHours(8);
        var maxAsistentes = 100;
        var status = "Publicado";

        // Act
        var dto = new EventoDto
        {
            Id = id,
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            MaxAsistentes = maxAsistentes,
            Status = status
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
        dto.StartDate.Should().Be(startDate);
        dto.EndDate.Should().Be(endDate);
        dto.MaxAsistentes.Should().Be(maxAsistentes);
        dto.Status.Should().Be(status);
    }

    [Fact]
    public void EventoDto_Location_ShouldBeSettable()
    {
        // Arrange
        var dto = new EventoDto();
        var locationDto = new LocationDto
        {
            VenueNombre = "Convention Center",
            Direccion = "123 Main St",
            Ciudad = "Tech Ciudad",
            State = "TC",
            ZipCode = "12345"
        };

        // Act
        dto.Location = locationDto;

        // Assert
        dto.Location.Should().NotBeNull();
        dto.Location.VenueNombre.Should().Be("Convention Center");
        dto.Location.Ciudad.Should().Be("Tech Ciudad");
    }

    [Fact]
    public void EventoDto_Asistentes_ShouldBeSettableAndEnumerable()
    {
        // Arrange
        var dto = new EventoDto();
        var asistentes = new List<AsistenteDto>
        {
            new AsistenteDto { Id = Guid.NewGuid(), Nombre = "John Doe", Email = "john@prueba.com" },
            new AsistenteDto { Id = Guid.NewGuid(), Nombre = "Jane Smith", Email = "jane@prueba.com" }
        };

        // Act
        dto.Asistentes = asistentes;

        // Assert
        dto.Asistentes.Should().HaveCount(2);
        dto.Asistentes.Should().Contain(a => a.Nombre == "John Doe");
        dto.Asistentes.Should().Contain(a => a.Nombre == "Jane Smith");
    }

    [Fact]
    public void EventoDto_ShouldAllowNullValues()
    {
        // Arrange & Act
        var dto = new EventoDto
        {
            Description = null,
            Location = null,
            Asistentes = null
        };

        // Assert
        dto.Description.Should().BeNull();
        dto.Location.Should().BeNull();
        dto.Asistentes.Should().BeNull();
    }
}
*/