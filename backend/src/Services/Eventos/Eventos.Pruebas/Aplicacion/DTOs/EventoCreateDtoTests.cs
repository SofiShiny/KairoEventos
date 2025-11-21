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
}
