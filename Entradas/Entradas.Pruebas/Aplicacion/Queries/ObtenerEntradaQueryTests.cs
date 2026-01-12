using Entradas.Aplicacion.Queries;
using FluentAssertions;
using Xunit;

namespace Entradas.Pruebas.Aplicacion.Queries;

public class ObtenerEntradaQueryTests
{
    [Fact]
    public void ObtenerEntradaQuery_Constructor_DebeAsignarIdCorrectamente()
    {
        // Arrange
        var entradaId = Guid.NewGuid();

        // Act
        var query = new ObtenerEntradaQuery(entradaId);

        // Assert
        query.EntradaId.Should().Be(entradaId);
    }

    [Fact]
    public void ObtenerEntradasPorUsuarioQuery_Constructor_DebeAsignarUsuarioIdCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        // Assert
        query.UsuarioId.Should().Be(usuarioId);
    }
}