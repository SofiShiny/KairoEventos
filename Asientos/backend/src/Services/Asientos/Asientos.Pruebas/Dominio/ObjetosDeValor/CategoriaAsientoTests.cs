using Asientos.Dominio.ObjetosDeValor;
using System;
using Xunit;

namespace Asientos.Pruebas.Dominio.ObjetosDeValor;

public class CategoriaAsientoTests
{
    [Fact]
    public void Crear_DebeCrearCategoriaConNombreValido()
    {
        // Arrange
        var nombre = "VIP";
        var precio = 150.50m;
        var prioridad = true;

        // Act
        var categoria = CategoriaAsiento.Crear(nombre, precio, prioridad);

        // Assert
        Assert.Equal(nombre, categoria.Nombre);
        Assert.Equal(precio, categoria.PrecioBase);
        Assert.True(categoria.TienePrioridad);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Crear_DebeLanzarExcepcionSiNombreEsInvalido(string nombre)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CategoriaAsiento.Crear(nombre, null, false));
    }

    [Fact]
    public void Igualdad_MismoNombre_Igual()
    {
        // Arrange
        var a = CategoriaAsiento.Crear("Gold", 100m, false);
        var b = CategoriaAsiento.Crear("gold", 100m, false);

        // Act & Assert
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Igualdad_NombreDistinto_NoIgual()
    {
        // Arrange
        var a = CategoriaAsiento.Crear("Gold", 100m, false);
        var b = CategoriaAsiento.Crear("Silver", 80m, false);

        // Act & Assert
        Assert.False(a.Equals(b));
    }
}

