using Eventos.Aplicacion.Comandos;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class EliminarEventoComandoTests
{
 [Fact]
 public void Constructor_InicializaEventoId()
 {
 var id = Guid.NewGuid();
 var cmd = new EliminarEventoComando(id);
 cmd.EventoId.Should().Be(id);
 }
}
