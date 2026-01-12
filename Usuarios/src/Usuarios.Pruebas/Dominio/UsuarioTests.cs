using FluentAssertions;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Dominio
{
    public class UsuarioTests
    {
        [Fact]
        public void Crear_UsuarioConDatosValidos_EstablecePropiedadesCorrectamente()
        {
            // Arrange
            var username = "testuser";
            var nombre = "Test User";
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Principal 123");
            var rol = Rol.User;
            
            // Act
            var usuario = Usuario.Crear(username, nombre, correo, telefono, direccion, rol);
            
            // Assert
            usuario.Id.Should().NotBeEmpty();
            usuario.Username.Should().Be(username);
            usuario.Nombre.Should().Be(nombre);
            usuario.Correo.Should().Be(correo);
            usuario.Telefono.Should().Be(telefono);
            usuario.Direccion.Should().Be(direccion);
            usuario.Rol.Should().Be(rol);
            usuario.EstaActivo.Should().BeTrue();
            usuario.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            usuario.FechaActualizacion.Should().BeNull();
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Crear_UsuarioConUsernameVacio_LanzaExcepcion(string usernameVacio)
        {
            // Arrange
            var nombre = "Test User";
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Principal 123");
            var rol = Rol.User;
            
            // Act
            Action act = () => Usuario.Crear(usernameVacio, nombre, correo, telefono, direccion, rol);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("El username no puede estar vacío");
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Crear_UsuarioConNombreVacio_LanzaExcepcion(string nombreVacio)
        {
            // Arrange
            var username = "testuser";
            var correo = Correo.Crear("test@example.com");
            var telefono = Telefono.Crear("1234567890");
            var direccion = Direccion.Crear("Calle Principal 123");
            var rol = Rol.User;
            
            // Act
            Action act = () => Usuario.Crear(username, nombreVacio, correo, telefono, direccion, rol);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("El nombre no puede estar vacío");
        }
        
        [Fact]
        public void Actualizar_Usuario_ModificaPropiedadesYFechaActualizacion()
        {
            // Arrange
            var usuario = CrearUsuarioValido();
            var nuevoNombre = "Nombre Actualizado";
            var nuevoTelefono = Telefono.Crear("9876543210");
            var nuevaDireccion = Direccion.Crear("Nueva Direccion 456");
            var fechaCreacionOriginal = usuario.FechaCreacion;
            
            // Act
            usuario.Actualizar(nuevoNombre, nuevoTelefono, nuevaDireccion);
            
            // Assert
            usuario.Nombre.Should().Be(nuevoNombre);
            usuario.Telefono.Should().Be(nuevoTelefono);
            usuario.Direccion.Should().Be(nuevaDireccion);
            usuario.FechaActualizacion.Should().NotBeNull();
            usuario.FechaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            usuario.FechaCreacion.Should().Be(fechaCreacionOriginal);
        }
        
        [Fact]
        public void CambiarRol_ModificaRolYFechaActualizacion()
        {
            // Arrange
            var usuario = CrearUsuarioValido();
            var nuevoRol = Rol.Admin;
            
            // Act
            usuario.CambiarRol(nuevoRol);
            
            // Assert
            usuario.Rol.Should().Be(nuevoRol);
            usuario.FechaActualizacion.Should().NotBeNull();
            usuario.FechaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void Desactivar_EstableceEstaActivoEnFalse()
        {
            // Arrange
            var usuario = CrearUsuarioValido();
            
            // Act
            usuario.Desactivar();
            
            // Assert
            usuario.EstaActivo.Should().BeFalse();
            usuario.FechaActualizacion.Should().NotBeNull();
            usuario.FechaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void Reactivar_EstableceEstaActivoEnTrue()
        {
            // Arrange
            var usuario = CrearUsuarioValido();
            usuario.Desactivar();
            
            // Act
            usuario.Reactivar();
            
            // Assert
            usuario.EstaActivo.Should().BeTrue();
            usuario.FechaActualizacion.Should().NotBeNull();
            usuario.FechaActualizacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        private Usuario CrearUsuarioValido()
        {
            return Usuario.Crear(
                "testuser",
                "Test User",
                Correo.Crear("test@example.com"),
                Telefono.Crear("1234567890"),
                Direccion.Crear("Calle Principal 123"),
                Rol.User
            );
        }
    }
}
