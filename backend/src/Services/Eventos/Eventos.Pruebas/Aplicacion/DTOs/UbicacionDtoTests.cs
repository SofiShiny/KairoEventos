using System.ComponentModel.DataAnnotations;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class UbicacionDtoTests
{
    private readonly UbicacionDto _dto;
    private readonly UbicacionDto _valido;
    private readonly UbicacionDto _invalido;

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

        _valido = new UbicacionDto{ NombreLugar="Lugar", Direccion="Dir", Ciudad="Ciudad", Pais="Pais" };
        _invalido = new UbicacionDto();
    }

    private static IList<ValidationResult> Validar(object obj)
    {
        var ctx = new ValidationContext(obj, null, null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(obj, ctx, results, true);
        return results;
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

    [Fact]
    public void Valida_SinErrores()
    {
        var resultados = Validar(_valido);
        resultados.Should().BeEmpty();
    }

    [Fact]
    public void FaltanRequeridos_RegistraErrores()
    {
        var resultados = Validar(_invalido);
        resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.NombreLugar)));
        resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.Direccion)));
        resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.Ciudad)));
        resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.Pais)));
    }
}
