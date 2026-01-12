using FluentAssertions;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Dominio
{
    public class CorreoTests
    {
        [Fact]
        public void Crear_CorreoValido_NormalizaALowercase()
        {
            // Arrange
            var correoMayusculas = "TEST@EXAMPLE.COM";
            
            // Act
            var correo = Correo.Crear(correoMayusculas);
            
            // Assert
            correo.Valor.Should().Be("test@example.com");
        }
        
        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("test@")]
        [InlineData("test@.com")]
        public void Crear_CorreoInvalido_LanzaExcepcion(string correoInvalido)
        {
            // Act
            Action act = () => Correo.Crear(correoInvalido);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage($"El correo '{correoInvalido}' no es válido");
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Crear_CorreoVacio_LanzaExcepcion(string correoVacio)
        {
            // Act
            Action act = () => Correo.Crear(correoVacio);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("El correo no puede estar vacío");
        }
        
        [Fact]
        public void Correos_ConMismoValor_SonIguales()
        {
            // Arrange
            var correo1 = Correo.Crear("test@example.com");
            var correo2 = Correo.Crear("test@example.com");
            
            // Act & Assert
            correo1.Should().Be(correo2);
            (correo1 == correo2).Should().BeTrue();
        }
    }
}
