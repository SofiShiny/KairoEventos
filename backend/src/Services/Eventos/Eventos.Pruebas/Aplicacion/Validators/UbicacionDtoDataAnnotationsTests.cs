using System.ComponentModel.DataAnnotations;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators;

public class UbicacionDtoDataAnnotationsTests
{
 private static IList<ValidationResult> Validar(object obj)
 {
 var ctx = new ValidationContext(obj, null, null);
 var results = new List<ValidationResult>();
 Validator.TryValidateObject(obj, ctx, results, true);
 return results;
 }

 [Fact]
 public void UbicacionDto_Valida_SinErrores()
 {
 var dto = new UbicacionDto
 {
 NombreLugar = "Lugar",
 Direccion = "Dir",
 Ciudad = "Ciudad",
 Pais = "Pais"
 };
 var resultados = Validar(dto);
 resultados.Should().BeEmpty();
 }

 [Fact]
 public void UbicacionDto_FaltanRequeridos_RegistraErrores()
 {
 var dto = new UbicacionDto();
 var resultados = Validar(dto);
 resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.NombreLugar)));
 resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.Direccion)));
 resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.Ciudad)));
 resultados.Should().Contain(r => r.MemberNames.Contains(nameof(UbicacionDto.Pais)));
 }
}
