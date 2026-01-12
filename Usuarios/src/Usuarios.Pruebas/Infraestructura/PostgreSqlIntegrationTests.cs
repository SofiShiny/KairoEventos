using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Infraestructura.Persistencia;
using Usuarios.Infraestructura.Repositorios;

namespace Usuarios.Pruebas.Infraestructura
{
    public class PostgreSqlIntegrationTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        private UsuariosDbContext? _context;
        private RepositorioUsuarios? _repositorio;

        public PostgreSqlIntegrationTests()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("kairo_usuarios_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            var options = new DbContextOptionsBuilder<UsuariosDbContext>()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _context = new UsuariosDbContext(options);
            await _context.Database.EnsureCreatedAsync();

            var logger = new Mock<ILogger<RepositorioUsuarios>>();
            _repositorio = new RepositorioUsuarios(_context, logger.Object);
        }

        public async Task DisposeAsync()
        {
            if (_context != null)
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.DisposeAsync();
            }
            await _postgresContainer.DisposeAsync();
        }

        [Fact]
        public async Task CrearUsuario_DebeRecuperarloDePostgreSQLReal()
        {
            // Arrange
            var correo = Correo.Crear("integration@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Integration 123");
            
            var usuario = Usuario.Crear(
                "integrationuser",
                "Integration User",
                correo,
                telefono,
                direccion,
                Rol.Admin
            );

            // Act
            await _repositorio!.AgregarAsync(usuario);
            
            // Clear context to force database read
            _context!.ChangeTracker.Clear();
            
            var usuarioRecuperado = await _repositorio.ObtenerPorIdAsync(usuario.Id);

            // Assert
            usuarioRecuperado.Should().NotBeNull();
            usuarioRecuperado!.Id.Should().Be(usuario.Id);
            usuarioRecuperado.Username.Should().Be("integrationuser");
            usuarioRecuperado.Nombre.Should().Be("Integration User");
            usuarioRecuperado.Correo.Valor.Should().Be("integration@example.com");
            usuarioRecuperado.Telefono.Valor.Should().Be("1234567890");
            usuarioRecuperado.Direccion.Valor.Should().Be("Calle Integration 123");
            usuarioRecuperado.Rol.Should().Be(Rol.Admin);
            usuarioRecuperado.EstaActivo.Should().BeTrue();
        }

        [Fact]
        public async Task ValueObjects_DebenPersistirseYRecuperarseCorrectamente()
        {
            // Arrange
            var correo = Correo.Crear("valueobject@example.com");
            var telefono = Telefono.Crear("9876543210");
            var direccion = Direccion.Crear("Avenida Value Object 456");
            
            var usuario = Usuario.Crear(
                "valueobjectuser",
                "Value Object User",
                correo,
                telefono,
                direccion,
                Rol.Organizator
            );

            // Act
            await _repositorio!.AgregarAsync(usuario);
            
            // Clear context to force database read
            _context!.ChangeTracker.Clear();
            
            var usuarioRecuperado = await _repositorio.ObtenerPorIdAsync(usuario.Id);

            // Assert - Verificar que los Value Objects se persisten correctamente
            usuarioRecuperado.Should().NotBeNull();
            
            // Verificar Correo
            usuarioRecuperado!.Correo.Should().NotBeNull();
            usuarioRecuperado.Correo.Valor.Should().Be("valueobject@example.com");
            
            // Verificar Telefono
            usuarioRecuperado.Telefono.Should().NotBeNull();
            usuarioRecuperado.Telefono.Valor.Should().Be("9876543210");
            
            // Verificar Direccion
            usuarioRecuperado.Direccion.Should().NotBeNull();
            usuarioRecuperado.Direccion.Valor.Should().Be("Avenida Value Object 456");
        }

        [Fact]
        public async Task IndiceUnico_Username_DebePrevenirDuplicados()
        {
            // Arrange
            var correo1 = Correo.Crear("user1@example.com");
            var telefono1 = Telefono.Crear("1111111111");
            var direccion1 = Direccion.Crear("Calle 1");
            
            var usuario1 = Usuario.Crear(
                "duplicateuser",
                "User 1",
                correo1,
                telefono1,
                direccion1,
                Rol.User
            );
            
            var correo2 = Correo.Crear("user2@example.com");
            var telefono2 = Telefono.Crear("2222222222");
            var direccion2 = Direccion.Crear("Calle 2");
            
            var usuario2 = Usuario.Crear(
                "duplicateuser", // Mismo username
                "User 2",
                correo2,
                telefono2,
                direccion2,
                Rol.User
            );

            // Act
            await _repositorio!.AgregarAsync(usuario1);
            
            var act = async () => await _repositorio.AgregarAsync(usuario2);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task IndiceUnico_Correo_DebePrevenirDuplicados()
        {
            // Arrange
            var correo1 = Correo.Crear("duplicate@example.com");
            
            var telefono1 = Telefono.Crear("1111111111");
            var direccion1 = Direccion.Crear("Calle 1");
            
            var usuario1 = Usuario.Crear(
                "user1",
                "User 1",
                correo1,
                telefono1,
                direccion1,
                Rol.User
            );
            
            await _repositorio!.AgregarAsync(usuario1);
            
            // Clear context to avoid tracking issues
            _context!.ChangeTracker.Clear();
            
            // Create a new correo instance with the same value
            var correo2 = Correo.Crear("duplicate@example.com");
            var telefono2 = Telefono.Crear("2222222222");
            var direccion2 = Direccion.Crear("Calle 2");
            
            var usuario2 = Usuario.Crear(
                "user2",
                "User 2",
                correo2, // Mismo correo
                telefono2,
                direccion2,
                Rol.User
            );

            // Act
            var act = async () => await _repositorio.AgregarAsync(usuario2);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task EliminacionLogica_DebeMarcarUsuarioComoInactivo()
        {
            // Arrange
            var correo = Correo.Crear("logicaldelete@example.com");
            var telefono = Telefono.Crear("5555555555");
            var direccion = Direccion.Crear("Calle Delete 789");
            
            var usuario = Usuario.Crear(
                "deleteuser",
                "Delete User",
                correo,
                telefono,
                direccion,
                Rol.User
            );

            await _repositorio!.AgregarAsync(usuario);

            // Act - Eliminación lógica
            usuario.Desactivar();
            await _repositorio.ActualizarAsync(usuario);
            
            // Clear context to force database read
            _context!.ChangeTracker.Clear();

            // Assert
            var usuarioRecuperado = await _repositorio.ObtenerPorIdAsync(usuario.Id);
            usuarioRecuperado.Should().NotBeNull();
            usuarioRecuperado!.EstaActivo.Should().BeFalse();
            
            // Verificar que no aparece en usuarios activos
            var usuariosActivos = await _repositorio.ObtenerActivosAsync();
            usuariosActivos.Should().NotContain(u => u.Id == usuario.Id);
            
            // Pero sí aparece en todos los usuarios
            var todosUsuarios = await _repositorio.ObtenerTodosAsync();
            todosUsuarios.Should().Contain(u => u.Id == usuario.Id);
        }
    }
}
