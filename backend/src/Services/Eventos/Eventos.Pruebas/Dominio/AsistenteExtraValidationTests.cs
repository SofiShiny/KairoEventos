using Eventos.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class AsistenteValidationTests
{
 [Fact]
 public void Constructor_EventoIdVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.Empty, "u1", "Nombre", "a@b.com");
 act.Should().Throw<ArgumentException>().WithMessage("*evento*");
 }

 [Fact]
 public void Constructor_UsuarioIdVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), " ", "Nombre", "a@b.com");
 act.Should().Throw<ArgumentException>().WithMessage("*usuario*");
 }

 [Fact]
 public void Constructor_NombreVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), "u1", "", "a@b.com");
 act.Should().Throw<ArgumentException>().WithMessage("*nombre*");
 }

 [Fact]
 public void Constructor_CorreoVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), "u1", "Nombre", " ");
 act.Should().Throw<ArgumentException>().WithMessage("*correo*");
 }

 [Fact]
 public void Constructor_EmailInvalido_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), "u1", "Nombre", "not-an-email");
 act.Should().Throw<ArgumentException>().WithMessage("*email*");
 }
}
