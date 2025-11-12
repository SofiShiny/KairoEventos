using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators
{
 public class AsistenteCreateValidatorTests
 {
 [Fact]
 public void ValidAsistente_ShouldBeValid()
 {
 var dto = new AsistenteCreateDto { Nombre = "Luz", Correo = "luz@example.com" };
 var validator = new AsistenteCreateValidator();
 var result = validator.Validate(dto);
 result.IsValid.Should().BeTrue();
 }

 [Fact]
 public void InvalidEmail_ShouldHaveError()
 {
 var dto = new AsistenteCreateDto { Nombre = "Luz", Correo = "invalid-email" };
 var validator = new AsistenteCreateValidator();
 var result = validator.Validate(dto);
 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "Correo");
 }
 }
}
