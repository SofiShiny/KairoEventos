using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class ActualizarEventoComandoTests
{
 [Fact]
 public void Constructor_InicializaPropiedades()
 {
 var ubic = new UbicacionDto{ NombreLugar="L", Direccion="D", Ciudad="C", Pais="P" };
 var id = Guid.NewGuid();
 var cmd = new ActualizarEventoComando(id, "Titulo", "Desc", ubic, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2),50);
 cmd.EventoId.Should().Be(id);
 cmd.Titulo.Should().Be("Titulo");
 cmd.Descripcion.Should().Be("Desc");
 cmd.Ubicacion.Should().Be(ubic);
 cmd.FechaInicio.Should().NotBeNull();
 cmd.MaximoAsistentes.Should().Be(50);
 }
}
