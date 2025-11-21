using System.ComponentModel.DataAnnotations;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoCreateDtoDataAnnotationsTests
{
 private readonly EventoCreateDto _valido;

 public EventoCreateDtoDataAnnotationsTests()
 {
 _valido = new EventoCreateDto
 {
 Titulo = "Titulo",
 Descripcion = "Desc",
 Ubicacion = new UbicacionDto { NombreLugar = "Lugar", Direccion = "Dir", Ciudad = "Ciudad", Pais = "Pais" },
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(2),
 MaximoAsistentes =10
 };
 }

 private static IList<ValidationResult> Validar(object obj)
 {
 var ctx = new ValidationContext(obj, null, null);
 var results = new List<ValidationResult>();
 Validator.TryValidateObject(obj, ctx, results, true);
 return results;
 }

 [Fact]
 public void ObjetoValido_SinErrores()
 {
 var results = Validar(_valido);
 results.Should().BeEmpty();
 }

 [Fact]
 public void FaltaTitulo_RegistraError()
 {
 _valido.Titulo = null!;
 var results = Validar(_valido);
 results.Should().Contain(r => r.MemberNames.Contains(nameof(EventoCreateDto.Titulo)));
 }

 [Fact]
 public void FaltaUbicacion_RegistraError()
 {
 _valido.Ubicacion = null;
 var results = Validar(_valido);
 results.Should().Contain(r => r.MemberNames.Contains(nameof(EventoCreateDto.Ubicacion)));
 }

 [Fact]
 public void MaximoAsistentesCero_RegistraError()
 {
 _valido.MaximoAsistentes =0;
 var results = Validar(_valido);
 results.Should().Contain(r => r.MemberNames.Contains(nameof(EventoCreateDto.MaximoAsistentes)));
 }
}
