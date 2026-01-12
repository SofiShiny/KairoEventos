using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class RaizAgregadaGenericaActualizadoEnTests
{
 private class RootGen : RaizAgregada<int>
 {
 public RootGen(int id) : base(id) { }
 }

 [Fact]
 public void ActualizadoEn_InicialmenteNull_EnGenerica_CubreLinea10()
 {
 var root = new RootGen(1);
 root.ActualizadoEn.Should().BeNull();
 }
}
