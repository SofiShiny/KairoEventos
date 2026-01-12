using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators;

public class EventoCreateValidatorTests
{
 private readonly EventoCreateValidator _validator;
 private readonly EventoCreateDto _valido;

 public EventoCreateValidatorTests()
 {
 _validator = new EventoCreateValidator();
 _valido = new EventoCreateDto
 {
 Titulo = "Taller de Arte",
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(1).AddHours(2),
 MaximoAsistentes =10,
 Ubicacion = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad="C", Pais="P"},
 Asistentes = new List<AsistenteCreateDto>{ new(){ Nombre="Creonte", Correo="c@d.com" } }
 };
 }

 [Fact]
 public void DatosValidos_EsValido()
 {
 _validator.Validate(_valido).IsValid.Should().BeTrue();
 }

 [Fact]
 public void TituloFaltante_Error()
 {
 _valido.Titulo = string.Empty;
 var result = _validator.Validate(_valido);
 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
 }

 [Fact]
 public void FechaInicioMayorQueFin_Error()
 {
 _valido.FechaInicio = DateTime.UtcNow.AddDays(2);
 _valido.FechaFin = DateTime.UtcNow.AddDays(1);
 _validator.Validate(_valido).IsValid.Should().BeFalse();
 }

 [Fact]
 public void MaximoInvalido_Error()
 {
 _valido.MaximoAsistentes =0;
 var result = _validator.Validate(_valido);
 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "MaximoAsistentes");
 }

 [Fact]
 public void EmailAsistenteInvalido_Error()
 {
 _valido.Asistentes = new(){ new AsistenteCreateDto{ Nombre="Creonte", Correo="not-an-email" } };
 _validator.Validate(_valido).IsValid.Should().BeFalse();
 }

 [Fact]
 public void MaximoUno_Valido()
 {
 _valido.MaximoAsistentes =1;
 _validator.Validate(_valido).IsValid.Should().BeTrue();
 }

 [Fact]
 public void FechasIguales_Invalido()
 {
 var inicio = DateTime.UtcNow.AddDays(2);
 _valido.FechaInicio = inicio;
 _valido.FechaFin = inicio;
 _validator.Validate(_valido).IsValid.Should().BeFalse();
 }
}
