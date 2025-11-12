using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using System;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators
{
 public class EventoUpdateValidatorTests
 {
 [Fact]
 public void WhenBothDatesProvided_InvalidOrder_ShouldHaveError()
 {
 var dto = new EventoUpdateDto
 {
 FechaInicio = DateTime.UtcNow.AddDays(2),
 FechaFin = DateTime.UtcNow.AddDays(1)
 };

 var validator = new EventoUpdateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.ErrorMessage.Contains("FechaInicio") || e.PropertyName.Contains("FechaInicio"));
 }

 [Fact]
 public void WhenMaximoProvided_InvalidValue_ShouldHaveError()
 {
 var dto = new EventoUpdateDto
 {
 MaximoAsistentes =0
 };

 var validator = new EventoUpdateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "MaximoAsistentes");
 }
 }
}
