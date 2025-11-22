using System.ComponentModel.DataAnnotations;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoCreateDtoTests
{
 private readonly EventoCreateDto _dto;
 private readonly DateTime _inicio;
 private readonly DateTime _fin;
 private readonly UbicacionDto _ubic;
 private readonly EventoCreateDto _valido;

 public EventoCreateDtoTests()
 {
 _inicio = DateTime.UtcNow.AddDays(2);
 _fin = _inicio.AddDays(1);
 _ubic = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad="C", Pais="P" };
 _dto = new EventoCreateDto
 {
 Titulo = "Titulo",
 Descripcion = "Desc",
 Ubicacion = _ubic,
 FechaInicio = _inicio,
 FechaFin = _fin,
 MaximoAsistentes =25,
 Estado = "Borrador",
 Asistentes = new List<AsistenteCreateDto>{ new AsistenteCreateDto{ Id=Guid.NewGuid(), Nombre="N", Correo="a@b.com" } }
 };

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
 public void Propiedades_AsignadasCorrectamente()
 {
 _dto.Titulo.Should().Be("Titulo");
 _dto.Ubicacion.Should().Be(_ubic);
 _dto.MaximoAsistentes.Should().Be(25);
 _dto.Estado.Should().Be("Borrador");
 _dto.Asistentes.Should().HaveCount(1);
 }

 [Fact]
 public void Defaults_MaximoAsistentesUno()
 {
 var d = new EventoCreateDto();
 d.MaximoAsistentes.Should().Be(1);
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
