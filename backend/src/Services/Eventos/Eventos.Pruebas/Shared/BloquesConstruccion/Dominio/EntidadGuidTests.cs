using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class EntidadGuidTests
{
 private class EntidadGuidImpl : Entidad
 {
 public EntidadGuidImpl() { }
 }

 [Fact]
 public void Equals_MismoGuid_True()
 {
 var a = new EntidadGuidImpl();
 var b = new EntidadGuidImpl();
 // forzar mismo Id
 typeof(Entidad).GetProperty("Id")!.SetValue(b, a.Id);
 a.Equals(b).Should().BeTrue();
 }

 [Fact]
 public void Equals_DistintoGuid_False()
 {
 var a = new EntidadGuidImpl();
 var b = new EntidadGuidImpl();
 a.Equals(b).Should().BeFalse();
 }
}
