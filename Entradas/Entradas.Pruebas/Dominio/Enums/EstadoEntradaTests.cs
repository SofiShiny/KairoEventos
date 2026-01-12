using Entradas.Dominio.Enums;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Dominio.Enums;

public class EstadoEntradaTests
{
    [Fact]
    public void EstadoEntrada_DebeContenerTodosLosEstadosEsperados()
    {
        // Arrange & Act
        var estados = Enum.GetValues<EstadoEntrada>();

        // Assert
        estados.Should().Contain(EstadoEntrada.PendientePago);
        estados.Should().Contain(EstadoEntrada.Pagada);
        estados.Should().Contain(EstadoEntrada.Cancelada);
        estados.Should().Contain(EstadoEntrada.Usada);
    }

    [Theory]
    [InlineData(EstadoEntrada.PendientePago, "PendientePago")]
    [InlineData(EstadoEntrada.Pagada, "Pagada")]
    [InlineData(EstadoEntrada.Cancelada, "Cancelada")]
    [InlineData(EstadoEntrada.Usada, "Usada")]
    public void EstadoEntrada_ToString_DebeRetornarNombreCorrect(EstadoEntrada estado, string nombreEsperado)
    {
        // Act
        var resultado = estado.ToString();

        // Assert
        resultado.Should().Be(nombreEsperado);
    }
}