using Eventos.Dominio.Entidades;
using FluentAssertions;
using System.Reflection;
using System;
using Xunit;

namespace Eventos.Pruebas.Dominio.Entidades;

// ========== Pruebas de AsistenteTests.cs ==========

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
            .WithMessage("*correo*"); // ajusta al nombre real del par�metro
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CrearAsistente_ConEmailVacio_DeberiaLanzarExcepcion(string email)
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

// ========== Pruebas de AsistenteExtraValidationTests.cs ==========

public class AsistenteValidationTests
{
 [Fact]
 public void Constructor_EventoIdVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.Empty, "u1", "Nombre", "a@b.com");
 act.Should().Throw<ArgumentException>().WithMessage("*evento*");
 }

 [Fact]
 public void Constructor_UsuarioIdVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), " ", "Nombre", "a@b.com");
 act.Should().Throw<ArgumentException>().WithMessage("*usuario*");
 }

 [Fact]
 public void Constructor_NombreVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), "u1", "", "a@b.com");
 act.Should().Throw<ArgumentException>().WithMessage("*nombre*");
 }

 [Fact]
 public void Constructor_CorreoVacio_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), "u1", "Nombre", " ");
 act.Should().Throw<ArgumentException>().WithMessage("*correo*");
 }

 [Fact]
 public void Constructor_EmailInvalido_LanzaExcepcion()
 {
 Action act = () => new Asistente(Guid.NewGuid(), "u1", "Nombre", "not-an-email");
 act.Should().Throw<ArgumentException>().WithMessage("*email*");
 }
}

// ========== Pruebas de AsistentePrivateCtorTests.cs ==========

public class AsistentePrivateCtorTests
{
 [Fact]
 public void PrivateParameterlessConstructor_UsadoPorReflection_CubreLinea15()
 {
 var ctor = typeof(Asistente).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
 ctor.Should().NotBeNull();
 var inst = (Asistente)ctor!.Invoke(null);
 inst.Should().NotBeNull();
 inst.EventoId.Should().Be(Guid.Empty); // default
 inst.UsuarioId.Should().Be(string.Empty);
 inst.NombreUsuario.Should().Be(string.Empty);
 inst.Correo.Should().Be(string.Empty);
 inst.RegistradoEn.Should().Be(default);
 }
}
