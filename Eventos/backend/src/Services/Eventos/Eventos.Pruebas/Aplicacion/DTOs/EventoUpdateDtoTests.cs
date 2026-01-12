using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoUpdateDtoTests
{
 [Fact]
 public void SetTodasPropiedades_NoNulas()
 {
 var dto = new EventoUpdateDto
 {
 Titulo = "T",
 Descripcion = "D",
 Ubicacion = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad="C", Pais="P"},
 FechaInicio = DateTime.UtcNow.AddDays(1),
 FechaFin = DateTime.UtcNow.AddDays(2),
 MaximoAsistentes =10,
 Estado = "Publicado",
 Asistentes = new List<AsistenteUpdateDto>{ new AsistenteUpdateDto{ Nombre="N", Correo="a@b.com" } }
 };
 dto.Titulo.Should().Be("T");
 dto.Estado.Should().Be("Publicado");
 dto.Asistentes.Should().NotBeNull();
 dto.Asistentes!.Count.Should().Be(1);
 }

 [Fact]
 public void PropiedadesNulas_Permitidas()
 {
 var dto = new EventoUpdateDto();
 dto.Titulo.Should().BeNull();
 dto.Ubicacion.Should().BeNull();
 dto.Asistentes.Should().BeNull();
 dto.Estado.Should().BeNull();
 }

 [Fact]
 public void Estado_Asignado_CubreLinea15()
 {
 var dto = new EventoUpdateDto{ Estado = "Cancelado" };
 dto.Estado.Should().Be("Cancelado");
 }
}
