using Entradas.Aplicacion.Comandos;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Aplicacion.Comandos;

public class CrearEntradaCommandTests
{
    [Fact]
    public void CrearEntradaCommand_Constructor_DebeAsignarPropiedadesCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        // Act
        var comando = new CrearEntradaCommand(eventoId, usuarioId, asientoId);

        // Assert
        comando.EventoId.Should().Be(eventoId);
        comando.UsuarioId.Should().Be(usuarioId);
        comando.AsientoId.Should().Be(asientoId);
    }

    [Fact]
    public void CrearEntradaCommand_ConAsientoIdNulo_DebePermitirlo()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var comando = new CrearEntradaCommand(eventoId, usuarioId, null);

        // Assert
        comando.EventoId.Should().Be(eventoId);
        comando.UsuarioId.Should().Be(usuarioId);
        comando.AsientoId.Should().BeNull();
    }
}