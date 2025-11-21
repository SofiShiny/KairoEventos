using Eventos.Aplicacion.Comandos;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class PublicarEventoComandoTests
{
 [Fact]
 public void Constructor_InicializaEventoId()
 {
 var id = Guid.NewGuid();
 var cmd = new PublicarEventoComando(id);
 cmd.EventoId.Should().Be(id);
 }
}
