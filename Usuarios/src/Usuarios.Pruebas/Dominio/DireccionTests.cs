using FluentAssertions;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Dominio
{
    public class DireccionTests
    {
        [Theory]
        [InlineData("  Calle Principal 123  ", "Calle Principal 123")]
        [InlineData("   Avenida Libertad 456   ", "Avenida Libertad 456")]
        [InlineData("Calle 123", "Calle 123")]
        public void Crear_DireccionValida_TrimeaEspacios(string direccionConEspacios, string direccionEsperada)
        {
            // Act
            var direccion = Direccion.Crear(direccionConEspacios);
            
            // Assert
            direccion.Valor.Should().Be(direccionEsperada);
        }
        
        [Theory]
        [InlineData("1234")]
        [InlineData("ABC")]
        [InlineData("12")]
        public void Crear_DireccionConMenosDe5Caracteres_LanzaExcepcion(string direccionCorta)
        {
            // Act
            Action act = () => Direccion.Crear(direccionCorta);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("La dirección debe tener al menos 5 caracteres");
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        [InlineData(null)]
        public void Crear_DireccionVacia_LanzaExcepcion(string direccionVacia)
        {
            // Act
            Action act = () => Direccion.Crear(direccionVacia);
            
            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("La dirección no puede estar vacía");
        }
    }
}
