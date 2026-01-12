using Eventos.Dominio.ObjetosDeValor;
using Eventos.Pruebas.Shared;
using FluentAssertions;
using System.Reflection;
using System;
using Xunit;

namespace Eventos.Pruebas.Dominio.ObjetosValor;

// ========== Pruebas de UbicacionTests.cs ==========

public class UbicacionTests
{
    [Fact]
    public void CrearUbicacion_ConDatosValidos_DeberiaExitoso()
    {
        // Preparar
        var nombreLugar = "Calle7";
        var direccion = "El Marques";
        var ciudad = "Caracas";
        var region = "DF";
        var codigoPostal = "1073";
        var pais = "Venezuela";

        // Actuar
        var ubicacion = new Ubicacion(nombreLugar, direccion, ciudad, region, codigoPostal, pais);

        // Comprobar
        ubicacion.Should().NotBeNull();
        ubicacion.NombreLugar.Should().Be(nombreLugar);
        ubicacion.Direccion.Should().Be(direccion);
        ubicacion.Ciudad.Should().Be(ciudad);
        ubicacion.Region.Should().Be(region);
        ubicacion.CodigoPostal.Should().Be(codigoPostal);
        ubicacion.Pais.Should().Be(pais);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CrearUbicacion_ConNombreLugarInvalido_DeberiaLanzarExcepcion(string nombreLugar)
    {
        // Actuar
        Action act = () => new Ubicacion(nombreLugar, "El Marques", "Caracas", "DF", "1073", "Venezuela");

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*nombreLugar*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CrearUbicacion_ConCiudadInvalida_DeberiaLanzarExcepcion(string ciudad)
    {
        // Actuar
        Action act = () => new Ubicacion("Calle7", "El Marques", ciudad, "DF", "1073", "Venezuela");

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ciudad*");
    }

    [Fact]
    public void Equals_ConMismosValores_DeberiaRetornarTrue()
    {
        // Preparar
        var ubicacion1 = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var ubicacion2 = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");

        // Actuar
        var sonIguales = ubicacion1.Equals(ubicacion2);

        // Comprobar
        sonIguales.Should().BeTrue();
    }

    [Fact]
    public void Equals_ConValoresDiferentes_DeberiaRetornarFalse()
    {
        // Preparar
        var ubicacion1 = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var ubicacion2 = new Ubicacion("Centro Cultural", "Av Principal", "Maracay", "AR", "2101", "Venezuela");

        // Actuar
        var sonIguales = ubicacion1.Equals(ubicacion2);

        // Comprobar
        sonIguales.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ConMismosValores_DeberiaRetornarMismoHash()
    {
        // Preparar
        var ubicacion1 = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");
        var ubicacion2 = new Ubicacion("Calle7", "El Marques", "Caracas", "DF", "1073", "Venezuela");

        // Actuar
        var hash1 = ubicacion1.GetHashCode();
        var hash2 = ubicacion2.GetHashCode();

        // Comprobar
        hash1.Should().Be(hash2);
    }
}

// ========== Pruebas de UbicacionAdditionalTests.cs ==========

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

// ========== Pruebas de UbicacionExtraValidationTests.cs ==========

public class UbicacionValidationTests
{
 [Theory]
 [InlineData("")]
 [InlineData(" ")]
 public void DireccionInvalida_LanzaExcepcion(string dir)
 {
 Action act = () => new Ubicacion("Lugar", dir, "Ciudad", "Reg", "0000", "Pais");
 act.Should().Throw<ArgumentException>().WithMessage("*direccion*");
 }

 [Theory]
 [InlineData("")]
 [InlineData(" ")]
 public void PaisInvalido_LanzaExcepcion(string pais)
 {
 Action act = () => new Ubicacion("Lugar", "Direccion", "Ciudad", "Reg", "0000", pais);
 act.Should().Throw<ArgumentException>().WithMessage("*pais*");
 }
}
