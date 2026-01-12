using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;
using Usuarios.Aplicacion.Comandos;
using Usuarios.Infraestructura.Persistencia;
using Usuarios.Infraestructura.Repositorios;

namespace Usuarios.Pruebas.Propiedades;

/// <summary>
/// Feature: refactorizacion-usuarios, Property 9: Logging de Operaciones
/// Validates: Requirements 11.2, 11.3
/// 
/// For any operación (comando o query), debe existir una entrada de log correspondiente con timestamp y resultado.
/// </summary>
public class LoggingOperacionesPropiedadesTests : IDisposable
{
    private readonly UsuariosDbContext _context;
    private readonly Mock<ILogger<RepositorioUsuarios>> _loggerRepositorio;
    private readonly Mock<ILogger<AgregarUsuarioComandoHandler>> _loggerHandler;
    private readonly IRepositorioUsuarios _repositorio;

    public LoggingOperacionesPropiedadesTests()
    {
        var options = new DbContextOptionsBuilder<UsuariosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsuariosDbContext(options);
        _loggerRepositorio = new Mock<ILogger<RepositorioUsuarios>>();
        _loggerHandler = new Mock<ILogger<AgregarUsuarioComandoHandler>>();
        _repositorio = new RepositorioUsuarios(_context, _loggerRepositorio.Object);
    }

    [Property(MaxTest = 100)]
    public Property RepositorioRegistraLogAlAgregar()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange
                var correo = Correo.Crear($"{datosUsuario.Username}@test.com");
                var telefono = Telefono.Crear("1234567890");
                var direccion = Direccion.Crear("Calle Test 123");

                var usuario = Usuario.Crear(
                    datosUsuario.Username,
                    "Usuario Test",
                    correo,
                    telefono,
                    direccion,
                    Rol.User);

                // Act
                await _repositorio.AgregarAsync(usuario);

                // Assert: Verificar que se registró un log
                _loggerRepositorio.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usuario agregado")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once,
                    "debe registrarse un log al agregar un usuario");
            });
    }

    [Property(MaxTest = 100)]
    public Property RepositorioRegistraLogAlActualizar()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange
                var correo = Correo.Crear($"{datosUsuario.Username}@test.com");
                var telefono = Telefono.Crear("1234567890");
                var direccion = Direccion.Crear("Calle Test 123");

                var usuario = Usuario.Crear(
                    datosUsuario.Username,
                    "Usuario Test",
                    correo,
                    telefono,
                    direccion,
                    Rol.User);

                await _repositorio.AgregarAsync(usuario);
                _loggerRepositorio.Invocations.Clear(); // Limpiar logs anteriores

                // Act
                usuario.Actualizar("Nombre Actualizado", telefono, direccion);
                await _repositorio.ActualizarAsync(usuario);

                // Assert: Verificar que se registró un log
                _loggerRepositorio.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usuario actualizado")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once,
                    "debe registrarse un log al actualizar un usuario");
            });
    }

    [Property(MaxTest = 100)]
    public Property HandlerRegistraLogAlEjecutarComando()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange
                var mockServicioKeycloak = new Mock<IServicioKeycloak>();
                mockServicioKeycloak
                    .Setup(x => x.CrearUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Guid.NewGuid().ToString());

                var handler = new AgregarUsuarioComandoHandler(
                    _repositorio,
                    mockServicioKeycloak.Object,
                    _loggerHandler.Object);

                var comando = new AgregarUsuarioComando
                {
                    Username = datosUsuario.Username,
                    Nombre = "Usuario Test",
                    Correo = $"{datosUsuario.Username}@test.com",
                    Telefono = "1234567890",
                    Direccion = "Calle Test 123",
                    Rol = Rol.User,
                    Password = "Password123"
                };

                // Act
                await handler.Handle(comando, CancellationToken.None);

                // Assert: Verificar que se registró un log de inicio
                _loggerHandler.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Agregando usuario")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once,
                    "debe registrarse un log al iniciar la operación");

                // Assert: Verificar que se registró un log de éxito
                _loggerHandler.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usuario agregado exitosamente")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once,
                    "debe registrarse un log al completar exitosamente la operación");
            });
    }

    private static Arbitrary<DatosUsuario> GeneradorUsuarioValido()
    {
        return Arb.From(
            Gen.Elements("user", "admin", "test", "demo")
                .SelectMany(prefix =>
                    Gen.Choose(1, 999)
                        .Select(num => new DatosUsuario
                        {
                            Username = $"{prefix}{num}"
                        })));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    private class DatosUsuario
    {
        public string Username { get; set; } = string.Empty;
    }
}
