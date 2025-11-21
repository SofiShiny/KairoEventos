using BloquesConstruccion.Aplicacion.Comun;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Aplicacion;

public class ResultadoExtraTests
{
 [Fact]
 public void Falla_Generic_CreaConValorDefault()
 {
 var r = Resultado.Falla<int>("e");
 r.EsFallido.Should().BeTrue();
 r.Valor.Should().Be(0);
 }

 [Fact]
 public void DistintasInstancias_Falla_NoIgualesPorReferencia()
 {
 var a = Resultado.Falla("e1");
 var b = Resultado.Falla("e2");
 a.Should().NotBe(b);
 }
}
