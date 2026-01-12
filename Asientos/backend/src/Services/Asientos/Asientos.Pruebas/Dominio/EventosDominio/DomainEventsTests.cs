using Asientos.Dominio.EventosDominio;
using System;
using Xunit;

namespace Asientos.Pruebas.Dominio.EventosDominio;

public class DomainEventsTests
{
    [Fact]
    public void MapaAsientosCreadoEventoDominio_DebeInicializarCorrectamente()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        // Act
        var domainEvent = new MapaAsientosCreadoEventoDominio(mapaId, eventoId);

        // Assert
        Assert.Equal(mapaId, domainEvent.MapaId);
        Assert.Equal(eventoId, domainEvent.EventoId);
        Assert.Equal(mapaId, domainEvent.IdAgregado);
    }

    [Fact]
    public void AsientoReservadoEventoDominio_DebeInicializarCorrectamente()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var fila = 5;
        var numero = 10;

        // Act
        var domainEvent = new AsientoReservadoEventoDominio(mapaId, fila, numero);

        // Assert
        Assert.Equal(mapaId, domainEvent.MapaId);
        Assert.Equal(fila, domainEvent.Fila);
        Assert.Equal(numero, domainEvent.Numero);
        Assert.Equal(mapaId, domainEvent.IdAgregado);
    }

    [Fact]
    public void AsientoLiberadoEventoDominio_Init()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var fila = 2;
        var numero = 3;

        // Act
        var ev = new AsientoLiberadoEventoDominio(mapaId, Guid.NewGuid(), fila, numero);

        // Assert
        Assert.Equal(mapaId, ev.MapaId);
        Assert.Equal(fila, ev.Fila);
        Assert.Equal(numero, ev.Numero);
        Assert.Equal(mapaId, ev.IdAgregado);
    }

    [Fact]
    public void AsientoAgregadoEventoDominio_Init()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var fila = 1;
        var numero = 2;
        var categoria = "VIP";

        // Act
        var ev = new AsientoAgregadoEventoDominio(mapaId, fila, numero, categoria);

        // Assert
        Assert.Equal(mapaId, ev.MapaId);
        Assert.Equal(fila, ev.Fila);
        Assert.Equal(numero, ev.Numero);
        Assert.Equal(categoria, ev.Categoria);
        Assert.Equal(mapaId, ev.IdAgregado);
    }

    [Fact]
    public void CategoriaAgregadaEventoDominio_Init()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var nombreCategoria = "General";

        // Act
        var ev = new CategoriaAgregadaEventoDominio(mapaId, nombreCategoria);

        // Assert
        Assert.Equal(nombreCategoria, ev.NombreCategoria);
        Assert.Equal(mapaId, ev.MapaId);
        Assert.Equal(mapaId, ev.IdAgregado);
    }

    [Fact]
    public void AsientoLiberadoEventoDominio_MapaId_DebeRetornarValorCorrecto()
    {
        // Arrange
        var mapaId = Guid.NewGuid();
        var fila = 1;
        var numero = 1;
        var evento = new AsientoLiberadoEventoDominio(mapaId, Guid.NewGuid(), fila, numero);

        // Act
        var resultado = evento.MapaId;

        // Assert
        Assert.Equal(mapaId, resultado);
    }
}

