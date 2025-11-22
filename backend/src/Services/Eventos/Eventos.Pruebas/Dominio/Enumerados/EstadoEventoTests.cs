using FluentAssertions;
using Xunit;
using Eventos.Dominio.Enumeraciones;

namespace Eventos.Pruebas.Dominio.Enumerados;

public class EstadoEventoTests
{
    [Fact]
    public void EstadoEvento_Borrador_DeberiaSerCero()
    {
        // Arrange & Act
        var status = EstadoEvento.Borrador;

        // Assert
        status.Should().Be(EstadoEvento.Borrador);
        ((int)status).Should().Be(0);
    }

    [Fact]
    public void EstadoEvento_Publicado_DeberiaSerUno()
    {
        // Arrange & Act
        var status = EstadoEvento.Publicado;

        // Assert
        status.Should().Be(EstadoEvento.Publicado);
        ((int)status).Should().Be(1);
    }

    [Fact]
    public void EstadoEvento_Cancelado_DeberiaSerDos()
    {
        // Arrange & Act
        var status = EstadoEvento.Cancelado;

        // Assert
        status.Should().Be(EstadoEvento.Cancelado);
        ((int)status).Should().Be(2);
    }

    [Fact]
    public void EstadoEvento_Completado_DeberiaSerTres()
    {
        // Arrange & Act
        var status = EstadoEvento.Completado;

        // Assert
        status.Should().Be(EstadoEvento.Completado);
        ((int)status).Should().Be(3);
    }

    [Theory]
    [InlineData(EstadoEvento.Borrador, "Borrador")]
    [InlineData(EstadoEvento.Publicado, "Publicado")]
    [InlineData(EstadoEvento.Cancelado, "Cancelado")]
    [InlineData(EstadoEvento.Completado, "Completado")]
    public void EstadoEvento_DeberiaConvertirAString(EstadoEvento status, string expected)
    {
        // Arrange & Act
        var result = status.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EstadoEvento_DeberiaTenerValoresUnicos()
    {
        // Arrange & Act
        var draft = EstadoEvento.Borrador;
        var published = EstadoEvento.Publicado;

        // Assert
        draft.Should().NotBe(published);
        draft.Should().Be(EstadoEvento.Borrador);
    }

    [Fact]
    public void EstadoEvento_DeberiaTenerTodosLosValores()
    {
        // Arrange & Act
        var values = Enum.GetValues<EstadoEvento>();

        // Assert
        values.Should().HaveCount(4);
        values.Should().Contain(EstadoEvento.Borrador);
        values.Should().Contain(EstadoEvento.Publicado);
        values.Should().Contain(EstadoEvento.Cancelado);
        values.Should().Contain(EstadoEvento.Completado);
    }
}
