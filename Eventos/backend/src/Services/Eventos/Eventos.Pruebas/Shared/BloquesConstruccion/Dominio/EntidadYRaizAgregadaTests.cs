using System;
using FluentAssertions;
using Xunit;
using BloquesConstruccion.Dominio;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class EntidadYRaizAgregadaTests
{
 private class EntidadInt : Entidad<int>
 {
 public EntidadInt(int id) : base(id) { }
 }
 private class RootInt : RaizAgregada<int>
 {
 public RootInt(int id) : base(id) { }
 public void Emit(EventoDominio e) => GenerarEventoDominio(e);
 }
 private class Evt : EventoDominio { public Evt(Guid id){ IdAgregado = id; } }

 [Fact]
 public void Entidad_Equals_PorId()
 {
 var a = new EntidadInt(1);
 var b = new EntidadInt(1);
 a.Equals(b).Should().BeTrue();
 a.GetHashCode().Should().Be(b.GetHashCode());
 }

 [Fact]
 public void RaizAgregada_EventosDominio_AgregarYLimpiar()
 {
 var r = new RootInt(5);
 r.Emit(new Evt(Guid.NewGuid()));
 r.EventosDominio.Should().HaveCount(1);
 r.LimpiarEventosDominio();
 r.EventosDominio.Should().BeEmpty();
 }

 [Fact]
 public void RaizAgregadaSinGenerico_EmitirYLimpiar()
 {
 var r = new RootNoGeneric();
 r.Emit(new Evt(Guid.NewGuid()));
 r.EventosDominio.Should().HaveCount(1);
 r.LimpiarEventosDominio();
 r.EventosDominio.Should().BeEmpty();
 }

 private class RootNoGeneric : RaizAgregada
 {
 public void Emit(EventoDominio e) => GenerarEventoDominio(e);
 }
}
