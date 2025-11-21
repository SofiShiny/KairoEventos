using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class RaizAgregadaActualizadoEnTests
{
 private class RootConcrete : RaizAgregada { }

 [Fact]
 public void ActualizadoEn_InicialmenteNull_CubreLinea40()
 {
 var root = new RootConcrete();
 root.ActualizadoEn.Should().BeNull();
 }
}
