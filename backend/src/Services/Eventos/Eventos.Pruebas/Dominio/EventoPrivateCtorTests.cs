using System;
using System.Reflection;
using Eventos.Dominio.Entidades;
using FluentAssertions;
using Xunit;
using Eventos.Pruebas.Shared;

namespace Eventos.Pruebas.Dominio;

public class EventoPrivateCtorTests
{
 [Fact]
 public void PrivateParameterlessConstructor_CreaInstancia()
 {
 var ctor = typeof(Evento).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
 ctor.Should().NotBeNull();
 var inst = (Evento)ctor!.Invoke(null);
 inst.Should().NotBeNull();
 inst.Titulo.Should().Be(string.Empty);
 }

 [Fact]
 public void BuildEventoHelper_CreaValido()
 {
 var ev = TestHelpers.BuildEvento();
 ev.Titulo.Should().Be("Titulo");
 ev.EstaPublicado.Should().BeFalse();
 }
}
