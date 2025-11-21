using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class UbicacionDtoTests
{
    private readonly UbicacionDto _dto;

    public UbicacionDtoTests()
    {
        _dto = new UbicacionDto
        {
            NombreLugar = "Centro",
            Direccion = "Av Principal",
            Ciudad = "Ciudad",
            Region = "Region",
            CodigoPostal = "0000",
            Pais = "Pais"
        };
    }

    [Fact]
    public void Propiedades_Asignadas()
    {
        _dto.NombreLugar.Should().Be("Centro");
        _dto.Pais.Should().Be("Pais");
    }

    [Fact]
    public void PermiteVaciosYNulos()
    {
        var d = new UbicacionDto();
        d.NombreLugar.Should().BeNull();
        d.Pais.Should().BeNull();
    }
}
