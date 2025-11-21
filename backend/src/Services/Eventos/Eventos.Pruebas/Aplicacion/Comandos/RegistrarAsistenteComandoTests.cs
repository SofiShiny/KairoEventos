using Eventos.Aplicacion.Comandos;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class RegistrarAsistenteComandoTests
{
 [Fact]
 public void Constructor_InicializaPropiedades()
 {
 var id = Guid.NewGuid();
 var cmd = new RegistrarAsistenteComando(id, "user-1", "Nombre Usuario", "a@b.com");
 cmd.EventoId.Should().Be(id);
 cmd.UsuarioId.Should().Be("user-1");
 cmd.NombreUsuario.Should().Be("Nombre Usuario");
 cmd.Correo.Should().Be("a@b.com");
 }
}
