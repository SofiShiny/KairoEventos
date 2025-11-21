using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoResponseDtoTests
{
 private readonly EventoResponseDto _dtoBase;
 private readonly Guid _id;
 private readonly DateTime _inicio;
 private readonly DateTime _fin;

 public EventoResponseDtoTests()
 {
 _id = Guid.NewGuid();
 _inicio = DateTime.UtcNow.AddDays(1);
 _fin = _inicio.AddDays(1);
 _dtoBase = new EventoResponseDto
 {
 Id = _id,
 Titulo = "T",
 Descripcion = "D",
 Ubicacion = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad="C", Pais="P"},
 FechaInicio = _inicio,
 FechaFin = _fin,
 MaximoAsistentes =50,
 ConteoAsistentesActual =5,
 Estado = "Publicado",
 OrganizadorId = "org-1",
 CreadoEn = DateTime.UtcNow,
 Asistentes = new[]{ new AsistenteResponseDto{ Id=Guid.NewGuid(), Nombre="N", Correo="a@b.com", RegistradoEn=DateTime.UtcNow } }
 };
 }

 [Fact]
 public void SetPropiedades_Completas()
 {
 _dtoBase.Estado.Should().Be("Publicado");
 _dtoBase.Asistentes.Should().HaveCount(1);
 }
}
