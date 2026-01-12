using System;
using Streaming.Aplicacion.DTOs;
using Eventos.Domain.Events;
using Streaming.Dominio.Modelos;
using Xunit;
using FluentAssertions;

namespace Streaming.Pruebas.Aplicacion.DTOs;

public class DtoTests
{
    [Fact]
    public void TransmisionDto_DebeMantenerValores()
    {
        // Arrange
        var id = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var plataforma = PlataformaTransmision.GoogleMeet;
        var url = "https://meet.google.com/abc-defg-hij";
        var estado = EstadoTransmision.EnVivo;

        // Act
        var dto = new TransmisionDto(id, eventoId, plataforma, url, estado);

        // Assert
        dto.Id.Should().Be(id);
        dto.EventoId.Should().Be(eventoId);
        dto.Plataforma.Should().Be(plataforma);
        dto.UrlAcceso.Should().Be(url);
        dto.Estado.Should().Be(estado);
    }

    [Fact]
    public void EventoPublicadoEventoDominio_DebeMantenerValores()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var nombre = "Test Event";
        var fecha = DateTime.Now;
        var esOnline = true;
        var precio = 10.0m;

        // Act
        var evento = new EventoPublicadoEventoDominio(eventoId, nombre, fecha, esOnline, precio);

        // Assert
        evento.EventoId.Should().Be(eventoId);
        evento.Nombre.Should().Be(nombre);
        evento.Fecha.Should().Be(fecha);
        evento.EsOnline.Should().Be(esOnline);
        evento.PrecioBase.Should().Be(precio);
    }
}
