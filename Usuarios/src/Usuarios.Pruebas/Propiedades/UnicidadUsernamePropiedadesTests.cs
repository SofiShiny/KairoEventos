using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.Excepciones;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;
using Usuarios.Aplicacion.Comandos;
using Usuarios.Infraestructura.Persistencia;
using Usuarios.Infraestructura.Repositorios;

namespace Usuarios.Pruebas.Propiedades;

/// <summary>
/// Feature: refactorizacion-usuarios, Property 1: Unicidad de Username
/// Validates: Requirements 5.2
/// 
/// For any dos usuarios en el sistema, sus usernames deben ser diferentes (case-insensitive).
/// </summary>
public class UnicidadUsernamePropiedadesTests : IDisposable
{
    private readonly UsuariosDbContext _context;
    private readonly IRepositorioUsuarios _repositorio;

    public UnicidadUsernamePropiedadesTests()
    {
        var options = new DbContextOptionsBuilder<UsuariosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsuariosDbContext(options);
        var logger = new Mock<ILogger<RepositorioUsuarios>>();
        _repositorio = new RepositorioUsuarios(_context, logger.Object);
    }

    [Property(MaxTest = 100)]
    public Property NoSePuedenCrearDosUsuariosConMismoUsername()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange: Crear primer usuario
                var correo1 = Correo.Crear($"user1_{Guid.NewGuid()}@test.com");
                var telefono1 = Telefono.Crear("1234567890");
                var direccion1 = Direccion.Crear("Calle Test 123");

                var usuario1 = Usuario.Crear(
                    datosUsuario.Username,
                    "Usuario Uno",
                    correo1,
                    telefono1,
                    direccion1,
                    Rol.User);

                await _repositorio.AgregarAsync(usuario1);

                // Act & Assert: Intentar crear segundo usuario con mismo username
                var existeUsername = await _repositorio.ExisteUsernameAsync(datosUsuario.Username);

                existeUsername.Should().BeTrue("el username ya existe en el sistema");

                // Verificar que no se puede agregar otro usuario con el mismo username
                var correo2 = Correo.Crear($"user2_{Guid.NewGuid()}@test.com");
                var telefono2 = Telefono.Crear("9876543210");
                var direccion2 = Direccion.Crear("Avenida Test 456");

                var usuario2 = Usuario.Crear(
                    datosUsuario.Username, // Mismo username
                    "Usuario Dos",
                    correo2,
                    telefono2,
                    direccion2,
                    Rol.User);

                // Al intentar agregar, debería fallar por constraint de BD
                Func<Task> act = async () => await _repositorio.AgregarAsync(usuario2);
                await act.Should().ThrowAsync<Exception>("no se pueden crear dos usuarios con el mismo username");
            });
    }

    [Property(MaxTest = 100)]
    public Property UsernameEsCaseInsensitive()
    {
        return Prop.ForAll(
            GeneradorUsernameValido(),
            async (username) =>
            {
                // Arrange: Crear usuario con username en minúsculas
                var correo = Correo.Crear($"test_{Guid.NewGuid()}@test.com");
                var telefono = Telefono.Crear("1234567890");
                var direccion = Direccion.Crear("Calle Test 123");

                var usuario = Usuario.Crear(
                    username.ToLowerInvariant(),
                    "Usuario Test",
                    correo,
                    telefono,
                    direccion,
                    Rol.User);

                await _repositorio.AgregarAsync(usuario);

                // Act & Assert: Verificar que existe con diferentes variaciones de case
                var existeLower = await _repositorio.ExisteUsernameAsync(username.ToLowerInvariant());
                var existeUpper = await _repositorio.ExisteUsernameAsync(username.ToUpperInvariant());

                existeLower.Should().BeTrue("el username existe en minúsculas");
                existeUpper.Should().BeTrue("el username debe ser case-insensitive");
            });
    }

    private static Arbitrary<DatosUsuario> GeneradorUsuarioValido()
    {
        return Arb.From(
            from username in GeneradorUsernameValido().Generator
            select new DatosUsuario
            {
                Username = username
            });
    }

    private static Arbitrary<string> GeneradorUsernameValido()
    {
        return Arb.From(
            Gen.Elements("user", "admin", "test", "demo", "john", "jane", "alice", "bob")
                .SelectMany(prefix =>
                    Gen.Choose(1, 999)
                        .Select(num => $"{prefix}{num}")));
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
