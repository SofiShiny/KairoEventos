/*using Eventos.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class AsistenteTests
{
    [Fact]
    public void CreateAsistente_WithValidData_ShouldSucceed()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = "user-123";
        var nombreUsuario = "John Doe";
        var email = "user@example.com";

        // Act
        var asistente = new Asistente(eventId, userId, nombreUsuario, email);

        // Assert
        asistente.Should().NotBeNull();
        asistente.EventoId.Should().Be(eventId);
        asistente.UserId.Should().Be(userId);
        asistente.NombreUsuario.Should().Be(nombreUsuario);
        asistente.Email.Should().Be(email);
        asistente.RegistradoEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateAsistente_WithEmptyEventoId_ShouldThrowException()
    {
        // Arrange
        var userId = "user-123";
        var nombreUsuario = "John Doe";
        var email = "user@example.com";

        // Act
        Action act = () => new Asistente(Guid.Empty, userId, nombreUsuario, email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*EventoId*");
    }

    [Fact]
    public void CreateAsistente_WithEmptyUserId_ShouldThrowException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var nombreUsuario = "John Doe";
        var email = "user@example.com";

        // Act
        Action act = () => new Asistente(eventId, string.Empty, nombreUsuario, email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserId*");
    }

    [Fact]
    public void CreateAsistente_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = "user-123";
        var nombreUsuario = "John Doe";

        // Act
        Action act = () => new Asistente(eventId, userId, nombreUsuario, "invalid-email");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateAsistente_WithNullOrEmptyEmail_ShouldThrowException(string email)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = "user-123";
        var nombreUsuario = "John Doe";

        // Act
        Action act = () => new Asistente(eventId, userId, nombreUsuario, email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
*/