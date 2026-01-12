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
using Usuarios.Infraestructura.Persistencia;
using Usuarios.Infraestructura.Repositorios;

namespace Usuarios.Pruebas.Propiedades;

/// <summary>
/// Feature: refactorizacion-usuarios, Property 2: Unicidad de Correo
/// Validates: Requirements 5.1
/// 
/// For any dos usuarios en el sistema, sus correos electrónicos deben ser diferentes (case-insensitive).
/// </summary>
public class UnicidadCorreoPropiedadesTests : IDisposable
{
    private readonly UsuariosDbContext _context;
    private readonly IRepositorioUsuarios _repositorio;

    public UnicidadCorreoPropiedadesTests()
    {
        var options = new DbContextOptionsBuilder<UsuariosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsuariosDbContext(options);
        var logger = new Mock<ILogger<RepositorioUsuarios>>();
        _repositorio = new RepositorioUsuarios(_context, logger.Object);
    }

    [Property(MaxTest = 100)]
    public Property NoSePuedenCrearDosUsuariosConMismoCorreo()
    {
        return Prop.ForAll(
            GeneradorCorreoValido(),
            async (correoStr) =>
            {
                // Arrange: Crear primer usuario
                var correo1 = Correo.Crear(correoStr);
                var telefono1 = Telefono.Crear("1234567890");
                var direccion1 = Direccion.Crear("Calle Test 123");

                var usuario1 = Usuario.Crear(
                    $"user1_{Guid.NewGuid()}",
                    "Usuario Uno",
                    correo1,
                    telefono1,
                    direccion1,
                    Rol.User);

                await _repositorio.AgregarAsync(usuario1);

                // Act & Assert: Verificar que el correo existe
                var existeCorreo = await _repositorio.ExisteCorreoAsync(correo1);
                existeCorreo.Should().BeTrue("el correo ya existe en el sistema");

                // Intentar crear segundo usuario con mismo correo
                var correo2 = Correo.Crear(correoStr); // Mismo correo
                var telefono2 = Telefono.Crear("9876543210");
                var direccion2 = Direccion.Crear("Avenida Test 456");

                var usuario2 = Usuario.Crear(
                    $"user2_{Guid.NewGuid()}",
                    "Usuario Dos",
                    correo2,
                    telefono2,
                    direccion2,
                    Rol.User);

                // Al intentar agregar, debería fallar por constraint de BD
                Func<Task> act = async () => await _repositorio.AgregarAsync(usuario2);
                await act.Should().ThrowAsync<Exception>("no se pueden crear dos usuarios con el mismo correo");
            });
    }

    [Property(MaxTest = 100)]
    public Property CorreoEsCaseInsensitive()
    {
        return Prop.ForAll(
            GeneradorCorreoValido(),
            async (correoStr) =>
            {
                // Arrange: Crear usuario con correo en minúsculas
                var correo = Correo.Crear(correoStr.ToLowerInvariant());
                var telefono = Telefono.Crear("1234567890");
                var direccion = Direccion.Crear("Calle Test 123");

                var usuario = Usuario.Crear(
                    $"user_{Guid.NewGuid()}",
                    "Usuario Test",
                    correo,
                    telefono,
                    direccion,
                    Rol.User);

                await _repositorio.AgregarAsync(usuario);

                // Act & Assert: Verificar que existe con diferentes variaciones de case
                var correoUpper = Correo.Crear(correoStr.ToUpperInvariant());
                var existeUpper = await _repositorio.ExisteCorreoAsync(correoUpper);

                existeUpper.Should().BeTrue("el correo debe ser case-insensitive");
            });
    }

    [Property(MaxTest = 100)]
    public Property CorreosNormalizadosAMinusculas()
    {
        return Prop.ForAll(
            GeneradorCorreoValido(),
            async (correoStr) =>
            {
                // Arrange: Crear correo con mayúsculas
                var correoMixto = Correo.Crear(correoStr);
                
                // Act & Assert: Verificar que se normaliza a minúsculas
                correoMixto.Valor.Should().Be(correoStr.ToLowerInvariant(), 
                    "los correos deben normalizarse a minúsculas");
            });
    }

    private static Arbitrary<string> GeneradorCorreoValido()
    {
        return Arb.From(
            Gen.Elements("test", "user", "admin", "demo", "john", "jane")
                .SelectMany(prefix =>
                    Gen.Choose(1, 999)
                        .SelectMany(num =>
                            Gen.Elements("example.com", "test.com", "mail.com")
                                .Select(domain => $"{prefix}{num}@{domain}"))));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
