using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using System;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators;

public class EventoUpdateValidatorTests
{
 private readonly EventoUpdateValidator _validator;
 private readonly EventoUpdateDto _dto;

 public EventoUpdateValidatorTests()
 {
 _validator = new EventoUpdateValidator();
 _dto = new EventoUpdateDto();
 }

 [Fact]
 public void AmbasFechas_OrdenInvalido_Error()
 {
 _dto.FechaInicio = DateTime.UtcNow.AddDays(2);
 _dto.FechaFin = DateTime.UtcNow.AddDays(1);
 var result = _validator.Validate(_dto);
 result.IsValid.Should().BeFalse();
 }

 [Fact]
 public void MaximoInvalido_Error()
 {
 _dto.MaximoAsistentes =0;
 var result = _validator.Validate(_dto);
 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "MaximoAsistentes");
 }
}
