using System.ComponentModel.DataAnnotations;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class UbicacionDtoDataAnnotationsTests
{
 private readonly UbicacionDto _valido;
 private readonly UbicacionDto _invalido;

 public UbicacionDtoDataAnnotationsTests()
 {
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
