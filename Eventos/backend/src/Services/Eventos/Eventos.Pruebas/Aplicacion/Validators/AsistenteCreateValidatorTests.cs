using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators;

public class AsistenteCreateValidatorTests
{
 private readonly AsistenteCreateValidator _validator;
 private readonly AsistenteCreateDto _valido;
 private readonly AsistenteCreateDto _correoInvalido;

 public AsistenteCreateValidatorTests()
 {
 _validator = new AsistenteCreateValidator();
 _valido = new AsistenteCreateDto{ Nombre = "Creonte", Correo = "c@d.com" };
 _correoInvalido = new AsistenteCreateDto{ Nombre = "Creonte", Correo = "invalid-email" };
 }

 [Fact]
 public void AsistenteValido_EsValido()
 {
 _validator.Validate(_valido).IsValid.Should().BeTrue();
 }

 [Fact]
 public void EmailInvalido_TieneError()
 {
 _validator.Validate(_correoInvalido).IsValid.Should().BeFalse();
 }
}
