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
/// Feature: refactorizacion-usuarios, Property 6: Eliminación Lógica
/// Validates: Requirements 5.5
/// 
/// For any usuario eliminado, su propiedad EstaActivo debe ser false y no debe aparecer en consultas de usuarios activos.
/// </summary>
public class EliminacionLogicaPropiedadesTests : IDisposable
{
    private readonly UsuariosDbContext _context;
    private readonly IRepositorioUsuarios _repositorio;

    public EliminacionLogicaPropiedadesTests()
    {
        var options = new DbContextOptionsBuilder<UsuariosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsuariosDbContext(options);
        var logger = new Mock<ILogger<RepositorioUsuarios>>();
        _repositorio = new RepositorioUsuarios(_context, logger.Object);
    }

    [Property(MaxTest = 100)]
    public Property UsuarioEliminadoTieneEstaActivoEnFalse()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange: Crear usuario
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

                // Act: Desactivar usuario
                usuario.Desactivar();
                await _repositorio.ActualizarAsync(usuario);

                // Assert
                var usuarioRecuperado = await _repositorio.ObtenerPorIdAsync(usuario.Id);
                usuarioRecuperado.Should().NotBeNull();
                usuarioRecuperado!.EstaActivo.Should().BeFalse("el usuario eliminado debe tener EstaActivo en false");
            });
    }

    [Property(MaxTest = 100)]
    public Property UsuarioEliminadoNoApareceEnObtenerActivos()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange: Crear usuario
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

                // Act: Desactivar usuario
                usuario.Desactivar();
                await _repositorio.ActualizarAsync(usuario);

                // Assert: No debe aparecer en usuarios activos
                var usuariosActivos = await _repositorio.ObtenerActivosAsync();
                usuariosActivos.Should().NotContain(u => u.Id == usuario.Id,
                    "el usuario eliminado no debe aparecer en la lista de usuarios activos");
            });
    }

    [Property(MaxTest = 100)]
    public Property UsuarioReactivadoApareceEnObtenerActivos()
    {
        return Prop.ForAll(
            GeneradorUsuarioValido(),
            async (datosUsuario) =>
            {
                // Arrange: Crear usuario
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

                // Act: Desactivar y luego reactivar
                usuario.Desactivar();
                await _repositorio.ActualizarAsync(usuario);
                
                usuario.Reactivar();
                await _repositorio.ActualizarAsync(usuario);

                // Assert: Debe aparecer en usuarios activos
                var usuariosActivos = await _repositorio.ObtenerActivosAsync();
                usuariosActivos.Should().Contain(u => u.Id == usuario.Id,
                    "el usuario reactivado debe aparecer en la lista de usuarios activos");
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
