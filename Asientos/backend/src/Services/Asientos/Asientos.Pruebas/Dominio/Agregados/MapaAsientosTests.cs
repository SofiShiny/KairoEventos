using Asientos.Dominio.Agregados;
using Asientos.Dominio.EventosDominio;
using Xunit;
using System;
using System.Linq;

namespace Asientos.Pruebas.Dominio.Agregados;

public class MapaAsientosTests
{
    [Fact]
    public void Crear_DebeCrearMapaConEventoIdValido()
    {
        // Arrange
        var eventoId = Guid.NewGuid();

        // Act
        var mapa = MapaAsientos.Crear(eventoId);

        // Assert
        Assert.NotNull(mapa);
        Assert.Equal(eventoId, mapa.EventoId);
        Assert.Single(mapa.Eventos);
        Assert.IsType<MapaAsientosCreadoEventoDominio>(mapa.Eventos.First());
    }

    [Fact]
    public void AgregarCategoria_DebeAgregarCategoriaCorrectamente()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        var nombreCategoria = "VIP";

        // Act
        var categoria = mapa.AgregarCategoria(nombreCategoria, 100, true);

        // Assert
        Assert.Single(mapa.Categorias);
        Assert.Equal(nombreCategoria, categoria.Nombre);
        Assert.True(categoria.TienePrioridad);
    }

    [Fact]
    public void AgregarCategoria_Duplicada_DebeFallar()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("VIP", 100m, true);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapa.AgregarCategoria("VIP", 100m, true));
    }

    [Fact]
    public void AgregarAsiento_DebeAgregarAsientoCorrectamente()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("General", 50m, false);

        // Act
        var asiento = mapa.AgregarAsiento(1, 1, "General");

        // Assert
        Assert.Single(mapa.Asientos);
        Assert.Equal(1, asiento.Fila);
        Assert.Equal(1, asiento.Numero);
    }

    [Fact]
    public void AgregarAsiento_Duplicado_DebeFallar()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("General", 50m, false);
        mapa.AgregarAsiento(1, 1, "General");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapa.AgregarAsiento(1, 1, "General"));
    }

    [Fact]
    public void ReservarAsiento_Inexistente_DebeFallar()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("General", 50m, false);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapa.ReservarAsiento(9, 9));
    }

    [Fact]
    public void LiberarAsiento_Inexistente_DebeFallar()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("General", 50m, false);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapa.LiberarAsiento(9, 9));
    }

    [Fact]
    public void ReservarYLiberarAsiento_FlujoCompleto()
    {
        // Arrange
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("General", 50m, false);
        var asiento = mapa.AgregarAsiento(2, 3, "General");

        // Act & Assert (Reservar)
        mapa.ReservarAsiento(2, 3);
        Assert.True(mapa.Asientos.First().Reservado);
        
        // Act & Assert (Liberar)
        mapa.LiberarAsiento(2, 3);
        Assert.False(mapa.Asientos.First().Reservado);

        // Reservar por ID
        mapa.ReservarAsientoPorId(asiento.Id, Guid.NewGuid());
        Assert.True(asiento.Reservado);

        // Liberar por ID
        mapa.LiberarAsientoPorId(asiento.Id);
        Assert.False(asiento.Reservado);
    }

    [Fact]
    public void ReservarAsientoPorId_YaReservadoPorOtro_Falla()
    {
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("VIP", 100, true);
        var asiento = mapa.AgregarAsiento(1, 1, "VIP");
        mapa.ReservarAsientoPorId(asiento.Id, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => mapa.ReservarAsientoPorId(asiento.Id, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_EventoIdVacio_Falla()
    {
        Assert.Throws<ArgumentException>(() => MapaAsientos.Crear(Guid.Empty));
    }

    [Fact]
    public void ReservarAsientoPorId_Inexistente_Falla()
    {
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => mapa.ReservarAsientoPorId(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void LiberarAsientoPorId_Inexistente_Falla()
    {
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => mapa.LiberarAsientoPorId(Guid.NewGuid()));
    }
}

