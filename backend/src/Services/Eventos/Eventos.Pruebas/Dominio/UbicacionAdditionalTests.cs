using System;
using System.Reflection;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Xunit;
using Eventos.Pruebas.Shared;

namespace Eventos.Pruebas.Dominio;

public class UbicacionAdditionalTests
{
 private readonly Ubicacion _valida;
 private readonly string _lugar = "Lugar";
 private readonly string _dir = "Dir";
 private readonly string _ciudad = "Ciudad";
 private readonly string _pais = "Pais";

 public UbicacionAdditionalTests()
 {
 _valida = TestHelpers.BuildUbicacion(_lugar,_dir,_ciudad,"Reg","CP",_pais);
 }

 [Fact]
 public void PrivateParameterlessConstructor_CreaInstancia()
 {
 var ctor = typeof(Ubicacion).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
 ctor.Should().NotBeNull();
 var inst = (Ubicacion)ctor!.Invoke(null);
 inst.Should().NotBeNull();
 }

 [Fact]
 public void FallbackRegionCodigoPostal_CadenaVacia()
 {
 var u = TestHelpers.BuildUbicacion(region:null!, codigoPostal:null!);
 u.Region.Should().Be(string.Empty);
 u.CodigoPostal.Should().Be(string.Empty);
 }

 [Fact]
 public void ToString_FormatoEsperado()
 {
 var str = _valida.ToString();
 str.Should().Contain(_lugar).And.Contain(_dir).And.Contain(_ciudad).And.Contain("Reg").And.Contain("CP").And.Contain(_pais);
 }
}
