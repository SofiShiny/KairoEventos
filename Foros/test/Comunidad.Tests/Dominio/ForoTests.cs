using Comunidad.Domain.Entidades;
using FluentAssertions;
using Xunit;

namespace Comunidad.Tests.Dominio;

public class ForoTests
{
    [Fact]
    public void Crear_GeneraIdYFechaCreacion()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var titulo = "Foro de Prueba";

        // Act
        var foro = Foro.Crear(eventoId, titulo);

        // Assert
        foro.Id.Should().NotBeEmpty();
        foro.EventoId.Should().Be(eventoId);
        foro.Titulo.Should().Be(titulo);
        foro.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_InicializaPropiedadesCorrectamente()
    {
        // Act
        var foro = new Foro();

        // Assert
        foro.Id.Should().NotBeEmpty();
        foro.FechaCreacion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        foro.Titulo.Should().BeEmpty();
    }

    [Fact]
    public void Crear_ConTituloVacio_CreaForoConTituloVacio()
    {
        // Arrange
        var eventoId = Guid.NewGuid();

        // Act
        var foro = Foro.Crear(eventoId, "");

        // Assert
        foro.Titulo.Should().BeEmpty();
        foro.EventoId.Should().Be(eventoId);
    }

    [Fact]
    public void Crear_GeneraIdsUnicos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();

        // Act
        var foro1 = Foro.Crear(eventoId, "Foro 1");
        var foro2 = Foro.Crear(eventoId, "Foro 2");

        // Assert
        foro1.Id.Should().NotBe(foro2.Id);
    }

    [Fact]
    public void Crear_ConEventoIdVacio_AsignaEventoIdVacio()
    {
        // Arrange
        var eventoId = Guid.Empty;

        // Act
        var foro = Foro.Crear(eventoId, "Foro");

        // Assert
        foro.EventoId.Should().Be(Guid.Empty);
    }
}
