using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class UbicacionDtoTests
{
    [Fact]
    public void UbicacionDto_DebeInicializarTodasLasPropiedades()
    {
        // Preparar & Ejecutar
        var dto = new UbicacionDto
        {
            NombreLugar = "Centro de Convenciones",
            Direccion = "Av Principal123",
            Ciudad = "Caracas",
            Region = "Distrito Capital",
            CodigoPostal = "1010",
            Pais = "Venezuela"
        };

        // Comprobar
        dto.NombreLugar.Should().Be("Centro de Convenciones");
        dto.Direccion.Should().Be("Av Principal123");
        dto.Ciudad.Should().Be("Caracas");
        dto.Region.Should().Be("Distrito Capital");
        dto.CodigoPostal.Should().Be("1010");
        dto.Pais.Should().Be("Venezuela");
    }

    [Fact]
    public void UbicacionDto_DebePermitirCadenasVacias()
    {
        // Preparar & Ejecutar
        var dto = new UbicacionDto
        {
            NombreLugar = string.Empty,
            Direccion = string.Empty,
            Ciudad = string.Empty,
            Region = string.Empty,
            CodigoPostal = string.Empty,
            Pais = string.Empty
        };

        // Comprobar
        dto.NombreLugar.Should().BeEmpty();
        dto.Direccion.Should().BeEmpty();
        dto.Ciudad.Should().BeEmpty();
        dto.Region.Should().BeEmpty();
        dto.CodigoPostal.Should().BeEmpty();
        dto.Pais.Should().BeEmpty();
    }
}
