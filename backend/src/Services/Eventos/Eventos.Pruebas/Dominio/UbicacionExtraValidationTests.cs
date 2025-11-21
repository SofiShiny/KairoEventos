using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class UbicacionValidationTests
{
 [Theory]
 [InlineData("")]
 [InlineData(" ")]
 public void DireccionInvalida_LanzaExcepcion(string dir)
 {
 Action act = () => new Ubicacion("Lugar", dir, "Ciudad", "Reg", "0000", "Pais");
 act.Should().Throw<ArgumentException>().WithMessage("*direccion*");
 }

 [Theory]
 [InlineData("")]
 [InlineData(" ")]
 public void PaisInvalido_LanzaExcepcion(string pais)
 {
 Action act = () => new Ubicacion("Lugar", "Direccion", "Ciudad", "Reg", "0000", pais);
 act.Should().Throw<ArgumentException>().WithMessage("*pais*");
 }
}
