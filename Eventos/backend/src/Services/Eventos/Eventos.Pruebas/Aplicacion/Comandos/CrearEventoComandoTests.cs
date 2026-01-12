using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.Comandos;

public class CrearEventoComandoTests
{
    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange
        var ubicacion = new UbicacionDto
        {
            NombreLugar = "Centro de Convenciones",
            Direccion = "Av. Principal123",
            Ciudad = "Caracas",
            Region = "DF",
            CodigoPostal = "1010",
            Pais = "Venezuela"
        };

        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = startDate.AddDays(2);

        // Act
        var comando = new CrearEventoComando(
            "Conferencia de Tecnologia",
            "Evento anual sobre innovacion",
            ubicacion,
            startDate,
            endDate,
            300,
            "org-001"
        );

        // Assert
        comando.Titulo.Should().Be("Conferencia de Tecnologia");
        comando.Descripcion.Should().Be("Evento anual sobre innovacion");
        comando.Ubicacion.Should().Be(ubicacion);
        comando.FechaInicio.Should().Be(startDate);
        comando.FechaFin.Should().Be(endDate);
        comando.MaximoAsistentes.Should().Be(300);
        comando.OrganizadorId.Should().Be("org-001");
    }
}