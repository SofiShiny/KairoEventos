using Eventos.Aplicacion.DTOs;
using Eventos.Aplicacion.Validators;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators
{
 public class EventoCreateValidatorTests
 {
 [Fact]
 public void ValidData_ShouldBeValid()
 {
 var dto = new EventoCreateDto
 {
 Titulo = "Concierto",
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(1).AddHours(2),
 MaximoAsistentes =10,
 Asistentes = new List<AsistenteCreateDto>
 {
 new AsistenteCreateDto { Nombre = "Juan", Correo = "juan@example.com" }
 }
 };

 var validator = new EventoCreateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeTrue();
 }

 [Fact]
 public void MissingTitulo_ShouldHaveError()
 {
 var dto = new EventoCreateDto
 {
 Titulo = string.Empty,
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(1).AddHours(2),
 MaximoAsistentes =5
 };

 var validator = new EventoCreateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
 }

 [Fact]
 public void FechaInicioAfterFechaFin_ShouldHaveError()
 {
 var dto = new EventoCreateDto
 {
 Titulo = "Evento",
 FechaInicio = DateTime.UtcNow.AddDays(2),
 FechaFin = DateTime.UtcNow.AddDays(1),
 MaximoAsistentes =5
 };

 var validator = new EventoCreateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "FechaInicio" || e.ErrorMessage.Contains("FechaInicio"));
 }

 [Fact]
 public void InvalidMaximoAsistentes_ShouldHaveError()
 {
 var dto = new EventoCreateDto
 {
 Titulo = "Evento",
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(1).AddHours(1),
 MaximoAsistentes =0
 };

 var validator = new EventoCreateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName == "MaximoAsistentes");
 }

 [Fact]
 public void InvalidAsistenteEmail_ShouldHaveError()
 {
 var dto = new EventoCreateDto
 {
 Titulo = "Evento",
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(1).AddHours(1),
 MaximoAsistentes =5,
 Asistentes = new List<AsistenteCreateDto>
 {
 new AsistenteCreateDto { Nombre = "Ana", Correo = "not-an-email" }
 }
 };

 var validator = new EventoCreateValidator();
 var result = validator.Validate(dto);

 result.IsValid.Should().BeFalse();
 result.Errors.Should().Contain(e => e.PropertyName.Contains("Asistentes"));
 }
 }
}
