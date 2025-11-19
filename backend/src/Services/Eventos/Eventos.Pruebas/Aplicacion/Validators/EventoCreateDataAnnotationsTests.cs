using System.ComponentModel.DataAnnotations;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Validators;

public class EventoCreateDataAnnotationsTests
{
 private EventoCreateDto CrearValido() => new()
 {
 Titulo = "Titulo",
 Descripcion = "Desc",
 Ubicacion = new UbicacionDto { NombreLugar = "Lugar", Direccion = "Dir", Ciudad = "Ciudad", Pais = "Pais" },
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(2),
 MaximoAsistentes =10
 };

 private static IList<ValidationResult> Validar(object obj)
 {
 var ctx = new ValidationContext(obj, null, null);
 var results = new List<ValidationResult>();
 Validator.TryValidateObject(obj, ctx, results, true);
 return results;
 }

 [Fact]
 public void DataAnnotations_ObjetoValido_SinErrores()
 {
 var dto = CrearValido();
 var results = Validar(dto);
 results.Should().BeEmpty();
 }

 [Fact]
 public void DataAnnotations_FaltaTitulo_RegistraError()
 {
 var dto = CrearValido();
 dto.Titulo = null!;
 var results = Validar(dto);
 results.Should().Contain(r => r.MemberNames.Contains(nameof(EventoCreateDto.Titulo)));
 }

 [Fact]
 public void DataAnnotations_FaltaUbicacion_RegistraError()
 {
 var dto = CrearValido();
 dto.Ubicacion = null;
 var results = Validar(dto);
 results.Should().Contain(r => r.MemberNames.Contains(nameof(EventoCreateDto.Ubicacion)));
 }

 [Fact]
 public void DataAnnotations_MaximoAsistentesCero_RegistraError()
 {
 var dto = CrearValido();
 dto.MaximoAsistentes =0;
 var results = Validar(dto);
 results.Should().Contain(r => r.MemberNames.Contains(nameof(EventoCreateDto.MaximoAsistentes)));
 }
}
