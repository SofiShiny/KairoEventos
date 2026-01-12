using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Infraestructura.Persistencia;
using Usuarios.Infraestructura.Repositorios;

namespace Usuarios.Pruebas.Infraestructura
{
    public class RepositorioUsuariosTests : IDisposable
    {
        private readonly UsuariosDbContext _context;
        private readonly RepositorioUsuarios _repositorio;

        public RepositorioUsuariosTests()
        {
            var options = new DbContextOptionsBuilder<UsuariosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuariosDbContext(options);
            var logger = new Mock<ILogger<RepositorioUsuarios>>();
            _repositorio = new RepositorioUsuarios(_context, logger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task AgregarAsync_DebePersisteUsuarioCorrectamente()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );

            // Act
            await _repositorio.AgregarAsync(usuario);

            // Assert
            var usuarioGuardado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuario.Id);
            usuarioGuardado.Should().NotBeNull();
            usuarioGuardado!.Username.Should().Be("testuser");
            usuarioGuardado.Nombre.Should().Be("Test User");
            usuarioGuardado.Correo.Valor.Should().Be("test@example.com");
            usuarioGuardado.Telefono.Valor.Should().Be("1234567890");
            usuarioGuardado.Direccion.Valor.Should().Be("Calle Test 123");
            usuarioGuardado.Rol.Should().Be(Rol.User);
            usuarioGuardado.EstaActivo.Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarUsuarioExistente()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );
            
            await _repositorio.AgregarAsync(usuario);

            // Act
            var resultado = await _repositorio.ObtenerPorIdAsync(usuario.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(usuario.Id);
            resultado.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarNullSiNoExiste()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();

            // Act
            var resultado = await _repositorio.ObtenerPorIdAsync(idInexistente);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorUsernameAsync_DebeRetornarUsuarioPorUsername()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );
            
            await _repositorio.AgregarAsync(usuario);

            // Act
            var resultado = await _repositorio.ObtenerPorUsernameAsync("testuser");

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task ObtenerPorUsernameAsync_DebeRetornarNullSiNoExiste()
        {
            // Act
            var resultado = await _repositorio.ObtenerPorUsernameAsync("usuarioinexistente");

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerPorCorreoAsync_DebeRetornarUsuarioPorCorreo()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );
            
            await _repositorio.AgregarAsync(usuario);

            // Act
            var correoBusqueda = Correo.Crear("test@example.com");
            var resultado = await _repositorio.ObtenerPorCorreoAsync(correoBusqueda);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Correo.Valor.Should().Be("test@example.com");
        }

        [Fact]
        public async Task ObtenerPorCorreoAsync_DebeRetornarNullSiNoExiste()
        {
            // Arrange
            var correoInexistente = Correo.Crear("inexistente@example.com");

            // Act
            var resultado = await _repositorio.ObtenerPorCorreoAsync(correoInexistente);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObtenerActivosAsync_SoloDebeRetornarUsuariosActivos()
        {
            // Arrange
            var correo1 = Correo.Crear("activo@example.com");
            var telefono1 = Telefono.Crear("1234567890");
            var direccion1 = Direccion.Crear("Calle Test 123");
            
            var usuarioActivo = Usuario.Crear(
                "usuarioactivo",
                "Usuario Activo",
                correo1,
                telefono1,
                direccion1,
                Rol.User
            );
            
            var correo2 = Correo.Crear("inactivo@example.com");
            var telefono2 = Telefono.Crear("0987654321");
            var direccion2 = Direccion.Crear("Calle Test 456");
            
            var usuarioInactivo = Usuario.Crear(
                "usuarioinactivo",
                "Usuario Inactivo",
                correo2,
                telefono2,
                direccion2,
                Rol.User
            );
            
            usuarioInactivo.Desactivar();
            
            await _repositorio.AgregarAsync(usuarioActivo);
            await _repositorio.AgregarAsync(usuarioInactivo);

            // Act
            var resultado = await _repositorio.ObtenerActivosAsync();

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Username.Should().Be("usuarioactivo");
        }

        [Fact]
        public async Task ExisteUsernameAsync_DebeRetornarTrueSiExiste()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );
            
            await _repositorio.AgregarAsync(usuario);

            // Act
            var resultado = await _repositorio.ExisteUsernameAsync("testuser");

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteUsernameAsync_DebeRetornarFalseSiNoExiste()
        {
            // Act
            var resultado = await _repositorio.ExisteUsernameAsync("usuarioinexistente");

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task ExisteCorreoAsync_DebeRetornarTrueSiExiste()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );
            
            await _repositorio.AgregarAsync(usuario);

            // Act
            var correoBusqueda = Correo.Crear("test@example.com");
            var resultado = await _repositorio.ExisteCorreoAsync(correoBusqueda);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteCorreoAsync_DebeRetornarFalseSiNoExiste()
        {
            // Arrange
            var correoInexistente = Correo.Crear("inexistente@example.com");

            // Act
            var resultado = await _repositorio.ExisteCorreoAsync(correoInexistente);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task ActualizarAsync_DebePersistirCambios()
        {
            // Arrange
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Test 123");
            
            var usuario = Usuario.Crear(
                "testuser",
                "Test User",
                correo,
                telefono,
                direccion,
                Rol.User
            );
            
            await _repositorio.AgregarAsync(usuario);

            // Act
            var nuevoTelefono = Telefono.Crear("9876543210");
            var nuevaDireccion = Direccion.Crear("Nueva Calle 456");
            usuario.Actualizar("Nombre Actualizado", nuevoTelefono, nuevaDireccion);
            
            await _repositorio.ActualizarAsync(usuario);

            // Assert
            var usuarioActualizado = await _repositorio.ObtenerPorIdAsync(usuario.Id);
            usuarioActualizado.Should().NotBeNull();
            usuarioActualizado!.Nombre.Should().Be("Nombre Actualizado");
            usuarioActualizado.Telefono.Valor.Should().Be("9876543210");
            usuarioActualizado.Direccion.Valor.Should().Be("Nueva Calle 456");
            usuarioActualizado.FechaActualizacion.Should().NotBeNull();
        }
    }
}
