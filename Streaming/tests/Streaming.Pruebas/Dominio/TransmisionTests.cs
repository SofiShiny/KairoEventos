using System;
using System.Linq;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Modelos;
using Xunit;
using FluentAssertions;

namespace Streaming.Pruebas.Dominio;

public class TransmisionTests
{
    [Fact]
    public void Crear_DebeGenerarUrlDeGoogleMeet_CuandoNoSeProveeUrl()
    {
        // Arrange
        var eventoId = Guid.NewGuid();

        // Act
        var transmision = Transmision.Crear(eventoId, PlataformaTransmision.GoogleMeet);

        // Assert
        transmision.UrlAcceso.Should().StartWith("https://meet.google.com/");
        transmision.UrlAcceso.Length.Should().BeGreaterThan(25);
        transmision.Estado.Should().Be(EstadoTransmision.Programada);
        transmision.EventoId.Should().Be(eventoId);
    }

    [Fact]
    public void Crear_DebeUsarUrlProveida_CuandoSeEspecifica()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var urlManual = "https://zoom.us/j/999888777";

        // Act
        var transmision = Transmision.Crear(eventoId, PlataformaTransmision.Zoom, urlManual);

        // Assert
        transmision.UrlAcceso.Should().Be(urlManual);
        transmision.Plataforma.Should().Be(PlataformaTransmision.Zoom);
    }

    [Fact]
    public void IniciarTransmision_DebeCambiarEstadoAEnVivo()
    {
        // Arrange
        var transmision = Transmision.Crear(Guid.NewGuid(), PlataformaTransmision.GoogleMeet);

        // Act
        transmision.IniciarTransmision();

        // Assert
        transmision.Estado.Should().Be(EstadoTransmision.EnVivo);
    }

    [Fact]
    public void FinalizarTransmision_DebeCambiarEstadoAFinalizada()
    {
        // Arrange
        var transmision = Transmision.Crear(Guid.NewGuid(), PlataformaTransmision.GoogleMeet);

        // Act
        transmision.FinalizarTransmision();

        // Assert
        transmision.Estado.Should().Be(EstadoTransmision.Finalizada);
    }

    [Fact]
    public void Transmision_ConstructorPrivado_DebeSerAccesiblePorReflection()
    {
        // Esto es para cobertura del constructor privado de EF Core
        var constructor = typeof(Transmision).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null, Array.Empty<Type>(), null);

        var transmision = (Transmision)constructor!.Invoke(null);

        transmision.Should().NotBeNull();
        transmision.Id.Should().BeEmpty();
    }
}
