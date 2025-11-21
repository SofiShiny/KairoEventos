using BloquesConstruccion.Aplicacion.Comun;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Aplicacion;

public class ResultadoGenericExitoTests
{
 [Fact]
 public void Exito_Generic_CubreLinea21()
 {
 var r = Resultado.Exito(42); // llama Resultado<T> Exito<T>
 r.EsExitoso.Should().BeTrue();
 r.Valor.Should().Be(42);
 r.Error.Should().BeEmpty();
 }
}
