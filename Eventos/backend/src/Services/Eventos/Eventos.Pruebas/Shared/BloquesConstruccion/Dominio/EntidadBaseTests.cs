using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class EntidadBaseTests
{
 private class EntidadGuidImpl : Entidad
 {
 }

 private class EntidadIntImpl : Entidad<int>
 {
 public EntidadIntImpl(int id) : base(id) {}
 }

 [Fact]
 public void EntidadGuid_IgualdadPorId()
 {
 var a = new EntidadGuidImpl();
 var b = new EntidadGuidImpl();
 b.GetType().GetProperty("Id")!.SetValue(b, a.Id);
 a.Equals(b).Should().BeTrue();
 a.GetHashCode().Should().Be(b.GetHashCode());
 }

 [Fact]
 public void EntidadGuid_DistintaPorId()
 {
 var a = new EntidadGuidImpl();
 var b = new EntidadGuidImpl();
 a.Equals(b).Should().BeFalse();
 }

 [Fact]
 public void EntidadInt_IgualdadPorId()
 {
 var a = new EntidadIntImpl(5);
 var b = new EntidadIntImpl(5);
 a.Equals(b).Should().BeTrue();
 a.GetHashCode().Should().Be(b.GetHashCode());
 }

 [Fact]
 public void EntidadInt_DistintaPorId()
 {
 var a = new EntidadIntImpl(1);
 var b = new EntidadIntImpl(2);
 a.Equals(b).Should().BeFalse();
 }
}
