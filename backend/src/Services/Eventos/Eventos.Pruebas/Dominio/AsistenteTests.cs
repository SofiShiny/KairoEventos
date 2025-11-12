using Eventos.Dominio.Entidades;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class AsistenteTests
{
    [Fact]
    public void CrearAsistente_ConDatosValidos_DeberiaTenerExito()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var userId = "usuario-001";
        var nombreUsuario = "Marcus Wilson";
        var email = "marcuswilson0929@gmail.com";

        // Act
        var asistente = new Asistente(eventId, userId, nombreUsuario, email);

        // Comprobar
        asistente.Should().NotBeNull();
        asistente.EventoId.Should().Be(eventId);
        asistente.UsuarioId.Should().Be(userId);
        asistente.NombreUsuario.Should().Be(nombreUsuario);
        asistente.Correo.Should().Be(email);
        asistente.RegistradoEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CrearAsistente_ConEventoIdVacio_DeberiaLanzarExcepcion()
    {
        // Preparar
        var userId = "usuario-002";
        var nombreUsuario = "Lilia Lara";
        var email = "lilia.lara0531@gmail.com";

        // Act
        Action act = () => new Asistente(Guid.Empty, userId, nombreUsuario, email);

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*EventoId*");
    }

    [Fact]
    public void CrearAsistente_ConUserIdVacio_DeberiaLanzarExcepcion()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var nombreUsuario = "Lilia Lara";
        var email = "lilialara@gmail.com";

        // Act
        Action act = () => new Asistente(eventId, string.Empty, nombreUsuario, email);

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*usuarioId*");
    }

    [Fact]
    public void CrearAsistente_ConEmailInvalido_DeberiaLanzarExcepcion()
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var userId = "usuario-002";
        var nombreUsuario = "LiliaLara";

        // Act
        Action act = () => new Asistente(eventId, userId, nombreUsuario, "invalid-email");

        // Comprobar
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CrearAsistente_ConEmailNuloOVacio_DeberiaLanzarExcepcion(string? email)
    {
        // Preparar
        var eventId = Guid.NewGuid();
        var userId = "usuario-002";
        var nombreUsuario = "Lilia Lara";

        // Act
        Action act = () => new Asistente(eventId, userId, nombreUsuario, email);

        // Comprobar
        act.Should().Throw<ArgumentException>();
    }
}