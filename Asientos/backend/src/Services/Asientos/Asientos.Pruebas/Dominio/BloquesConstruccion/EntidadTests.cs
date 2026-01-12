using BloquesConstruccion.Dominio;
using Xunit;
using System;
using System.Collections.Generic;

namespace Asientos.Pruebas.Dominio.BloquesConstruccion;

// Clase concreta para probar la clase abstracta Entidad
file class EntidadConcreta : Entidad { }

// Variante que permite forzar Id vacío
file class EntidadConcretaEmpty : Entidad { public EntidadConcretaEmpty() { Id = Guid.Empty; } }

// Variante que permite crear con Id específico
file class EntidadConcretaWithId : Entidad { public EntidadConcretaWithId(Guid id) { Id = id; } }

public class EntidadTests
{
    [Fact]
    public void Igualdad_DebeSerIgualSiTienenMismoReferencia()
    {
        // Arrange
        var entidad1 = new EntidadConcreta();
        var entidad2 = entidad1; // Misma referencia

        // Act & Assert
        Assert.True(entidad1.Equals(entidad2));
        Assert.Equal(entidad1.GetHashCode(), entidad2.GetHashCode());
    }

    [Fact]
    public void Igualdad_DebeSerFalsoSiTienenDiferenteId()
    {
        // Arrange
        var entidad1 = new EntidadConcreta();
        var entidad2 = new EntidadConcreta();

        // Act & Assert
        Assert.False(entidad1.Equals(entidad2));
        Assert.NotEqual(entidad1.GetHashCode(), entidad2.GetHashCode());
    }

    [Fact]
    public void Igualdad_DiferentesInstancias_MismoId_DebeSerIgual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a = new EntidadConcretaWithId(id);
        var b = new EntidadConcretaWithId(id);

        // Act & Assert
        Assert.False(ReferenceEquals(a, b));
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        // Arrange
        var entidad = new EntidadConcreta();

        // Act & Assert
        Assert.False(entidad.Equals(new object()));
    }

    [Fact]
    public void Equals_WhenEitherIdEmpty_ReturnsFalse()
    {
        // Arrange
        var empty = new EntidadConcretaEmpty();
        var normal = new EntidadConcreta();

        // Act & Assert
        Assert.False(empty.Equals(normal));
        Assert.False(normal.Equals(empty));
    }
}

// Agrego pruebas para ObjetoValor aquí para mantener 1:1 sin crear archivos nuevos
file class ObjetoValorConcreto : ObjetoValor
{
    public string Propiedad1 { get; }
    public int Propiedad2 { get; }

    public ObjetoValorConcreto(string p1, int p2) { Propiedad1 = p1; Propiedad2 = p2; }
    protected override IEnumerable<object> ObtenerComponentesDeIgualdad() { yield return Propiedad1; yield return Propiedad2; }
}

file class ObjetoValorConcretoNullable : ObjetoValor
{
    public string? Propiedad1 { get; }
    public int Propiedad2 { get; }
    public ObjetoValorConcretoNullable(string? p1, int p2) { Propiedad1 = p1; Propiedad2 = p2; }
    protected override IEnumerable<object> ObtenerComponentesDeIgualdad() { yield return Propiedad1; yield return Propiedad2; }
}

public class ObjetoValorInlineTests
{
    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var o = new ObjetoValorConcreto("x",1);
        Assert.False(o.Equals(new object()));
    }

    [Fact]
    public void GetHashCode_HandlesNullComponent_AndIsStable()
    {
        var a = new ObjetoValorConcretoNullable(null, 5);
        var b = new ObjetoValorConcretoNullable(null, 5);
        Assert.True(a.Equals(b));
        var ha = a.GetHashCode();
        var hb = b.GetHashCode();
        Assert.Equal(ha, hb);
        Assert.Equal(ha, a.GetHashCode()); // stable
    }

    [Fact]
    public void GetHashCode_DifferentValues_TypicallyDifferent()
    {
        var a = new ObjetoValorConcreto("A", 1);
        var b = new ObjetoValorConcreto("B", 1);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }
}
