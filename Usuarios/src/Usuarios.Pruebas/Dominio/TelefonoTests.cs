using FluentAssertions;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Dominio
{
    public class TelefonoTests
    {
        [Theory]
        [InlineData("+1 (555) 123-4567", "15551234567")]
        [InlineData("555-123-4567", "5551234567")]
        [InlineData("(555) 123 4567", "5551234567")]
        [InlineData("555.123.4567", "5551234567")]
        public void Crear_TelefonoValido_LimpiaCaracteresEspeciales(string telefonoConCaracteres, string telefonoEsperado)
        {
            // Act
            var telefono = Telefono.Crear(telefonoConCaracteres);
            
            // Assert
            telefono.Valor.Should().Be(telefonoEsperado);
        }
        
        [Theory]
        [InlineData("123456")]
        [InlineData("12345")]
        [InlineData("1")]
        public void Crear_TelefonoConMenosDe7Digitos_LanzaExcepcion(string telefonoCorto)
        {
            // Act
            Action act = () => Telefono.Crear(telefonoCorto);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("El teléfono debe tener entre 7 y 15 dígitos");
        }
        
        [Theory]
        [InlineData("1234567890123456")]
        [InlineData("12345678901234567890")]
        public void Crear_TelefonoConMasDe15Digitos_LanzaExcepcion(string telefonoLargo)
        {
            // Act
            Action act = () => Telefono.Crear(telefonoLargo);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("El teléfono debe tener entre 7 y 15 dígitos");
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Crear_TelefonoVacio_LanzaExcepcion(string telefonoVacio)
        {
            // Act
            Action act = () => Telefono.Crear(telefonoVacio);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("El teléfono no puede estar vacío");
        }
    }
}
