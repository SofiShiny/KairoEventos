using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class EntidadEqualsAdditionalTests
{
 private class EGuid : Entidad { }
 private class EInt : Entidad<int> { public EInt(int id):base(id){} }

 [Fact]
 public void EntidadGuid_Equals_Null_False()
 {
 var a = new EGuid();
 a.Equals(null).Should().BeFalse();
 }

 [Fact]
 public void EntidadGenerica_Equals_Null_False()
 {
 var a = new EInt(1);
 a.Equals(null).Should().BeFalse();
 }

 [Fact]
 public void EntidadGenerica_Equals_OtroTipo_False()
 {
 var a = new EInt(1);
 a.Equals(new object()).Should().BeFalse();
 }
}
