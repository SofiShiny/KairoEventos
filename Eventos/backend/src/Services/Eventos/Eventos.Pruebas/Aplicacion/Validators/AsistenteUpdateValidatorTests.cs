using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators;

public class AsistenteUpdateValidatorTests
{
 private readonly AsistenteUpdateValidator _validator;

 public AsistenteUpdateValidatorTests()
 {
 _validator = new AsistenteUpdateValidator();
 }

 [Fact]
 public void NombreVacio_Error()
 {
 var r = _validator.Validate(new AsistenteUpdateDto{ Nombre="" });
 r.IsValid.Should().BeFalse();
 }

 [Fact]
 public void CorreoInvalido_Error()
 {
 var r = _validator.Validate(new AsistenteUpdateDto{ Correo="bad" });
 r.IsValid.Should().BeFalse();
 }

 [Fact]
 public void CorreoValido_Ok()
 {
 var r = _validator.Validate(new AsistenteUpdateDto{ Correo="a@b.com" });
 r.IsValid.Should().BeTrue();
 }

 [Fact]
 public void NombreNull_SinError()
 {
 var r = _validator.Validate(new AsistenteUpdateDto{ Nombre=null });
 r.IsValid.Should().BeTrue();
 }
}
