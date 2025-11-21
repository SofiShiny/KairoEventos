using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class EntidadGenericaTests
{
 private class EntidadInt : Entidad<int>
 {
 public EntidadInt(int id) : base(id) {}
 }

 [Fact]
 public void Equals_MismoId_True()
 {
 var a = new EntidadInt(7);
 var b = new EntidadInt(7);
 a.Equals(b).Should().BeTrue();
 }

 [Fact]
 public void Equals_DistintoId_False()
 {
 var a = new EntidadInt(7);
 var b = new EntidadInt(8);
 a.Equals(b).Should().BeFalse();
 }

 [Fact]
 public void GetHashCode_MismoId_Igual()
 {
 var a = new EntidadInt(3);
 var b = new EntidadInt(3);
 a.GetHashCode().Should().Be(b.GetHashCode());
 }
}
