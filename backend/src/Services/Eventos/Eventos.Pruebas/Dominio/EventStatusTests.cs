/*using Eventos.Dominio.Enumerados;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class EventoStatusTests
{
    [Fact]
    public void EventoStatus_ShouldHaveDraftValue()
    {
        // Act
        var status = EventoStatus.Draft;

        // Assert
        status.Should().Be(EventoStatus.Draft);
        ((int)status).Should().Be(0);
    }

    [Fact]
    public void EventoStatus_ShouldHavePublishedValue()
    {
        // Act
        var status = EventoStatus.Publicado;

        // Assert
        status.Should().Be(EventoStatus.Publicado);
        ((int)status).Should().Be(1);
    }

    [Fact]
    public void EventoStatus_ShouldHaveCancelledValue()
    {
        // Act
        var status = EventoStatus.Cancelled;

        // Assert
        status.Should().Be(EventoStatus.Cancelled);
        ((int)status).Should().Be(2);
    }

    [Fact]
    public void EventoStatus_ShouldHaveCompletedValue()
    {
        // Act
        var status = EventoStatus.Completed;

        // Assert
        status.Should().Be(EventoStatus.Completed);
        ((int)status).Should().Be(3);
    }

    [Theory]
    [InlineData(EventoStatus.Draft, "Draft")]
    [InlineData(EventoStatus.Publicado, "Publicado")]
    [InlineData(EventoStatus.Cancelled, "Cancelled")]
    [InlineData(EventoStatus.Completed, "Completed")]
    public void EventoStatus_ShouldConvertToString(EventoStatus status, string expected)
    {
        // Act
        var result = status.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EventoStatus_ShouldAllowComparison()
    {
        // Arrange
        var draft = EventoStatus.Draft;
        var published = EventoStatus.Publicado;

        // Act & Assert
        draft.Should().NotBe(published);
        draft.Should().Be(EventoStatus.Draft);
    }

    [Fact]
    public void EventoStatus_ShouldSupportAllValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<EventoStatus>();

        // Assert
        values.Should().HaveCount(4);
        values.Should().Contain(EventoStatus.Draft);
        values.Should().Contain(EventoStatus.Publicado);
        values.Should().Contain(EventoStatus.Cancelled);
        values.Should().Contain(EventoStatus.Completed);
    }
}
*/