using System;
using System.Reflection;
using Eventos.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class AsistentePrivateCtorTests
{
 [Fact]
 public void PrivateParameterlessConstructor_UsadoPorReflection_CubreLinea15()
 {
 var ctor = typeof(Asistente).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
 ctor.Should().NotBeNull();
 var inst = (Asistente)ctor!.Invoke(null);
 inst.Should().NotBeNull();
 inst.EventoId.Should().Be(Guid.Empty); // default
 inst.UsuarioId.Should().Be(string.Empty);
 inst.NombreUsuario.Should().Be(string.Empty);
 inst.Correo.Should().Be(string.Empty);
 inst.RegistradoEn.Should().Be(default);
 }
}
