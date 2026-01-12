using Entradas.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Dominio;

public class EntidadBaseTests
{
    private class EntidadPrueba : EntidadBase
    {
        public string Nombre { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_DebeAsignarIdYFechaCreacion()
    {
        // Act
        var entidad = new EntidadPrueba();

        // Assert
        entidad.Id.Should().NotBe(Guid.Empty);
        entidad.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entidad.FechaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Id_DebeSerUnico()
    {
        // Act
        var entidad1 = new EntidadPrueba();
        var entidad2 = new EntidadPrueba();

        // Assert
        entidad1.Id.Should().NotBe(entidad2.Id);
    }
}