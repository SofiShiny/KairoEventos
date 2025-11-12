using Eventos.Dominio.Enumeraciones;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class EventoStatusTests
{
    [Fact]
    public void EventoStatus_DeberiaTenerValorDraft()
    {
        // Actuar
        var status = EstadoEvento.Borrador;

        // Comprobar
        status.Should().Be(EstadoEvento.Borrador);
        ((int)status).Should().Be(0);
    }

    [Fact]
    public void EventoStatus_DeberiaTenerValorPublicado()
    {
        // Actuar
        var status = EstadoEvento.Publicado;

        // Comprobar
        status.Should().Be(EstadoEvento.Publicado);
        ((int)status).Should().Be(1);
    }

    [Fact]
    public void EventoStatus_DeberiaTenerValorCancelado()
    {
        // Actuar
        var status = EstadoEvento.Cancelado;

        // Comprobar
        status.Should().Be(EstadoEvento.Cancelado);
        ((int)status).Should().Be(2);
    }

    [Fact]
    public void EventoStatus_DeberiaTenerValorCompletado()
    {
        // Actuar
        var status = EstadoEvento.Completado;

        // Comprobar
        status.Should().Be(EstadoEvento.Completado);
        ((int)status).Should().Be(3);
    }

    [Theory]
    [InlineData(EstadoEvento.Borrador, "Borrador")]
    [InlineData(EstadoEvento.Publicado, "Publicado")]
    [InlineData(EstadoEvento.Cancelado, "Cancelado")]
    [InlineData(EstadoEvento.Completado, "Completado")]
    public void EventoStatus_DeberiaConvertirAString(EstadoEvento status, string expected)
    {
        // Actuar
        var result = status.ToString();

        // Comprobar
        result.Should().Be(expected);
    }

    [Fact]
    public void EventoStatus_DeberiaPermitirComparacion()
    {
        // Preparar
        var draft = EstadoEvento.Borrador;
        var published = EstadoEvento.Publicado;

        // Actuar & Comprobar
        draft.Should().NotBe(published);
        draft.Should().Be(EstadoEvento.Borrador);
    }

    [Fact]
    public void EventoStatus_DeberiaSoportarTodosLosValores()
    {
        // Preparar & Actuar
        var values = Enum.GetValues<EstadoEvento>();

        // Comprobar
        values.Should().HaveCount(4);
        values.Should().Contain(EstadoEvento.Borrador);
        values.Should().Contain(EstadoEvento.Publicado);
        values.Should().Contain(EstadoEvento.Cancelado);
        values.Should().Contain(EstadoEvento.Completado);
    }
}