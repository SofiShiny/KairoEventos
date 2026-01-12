using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Modelos;
using Streaming.Infraestructura.Persistencia;
using Xunit;
using FluentAssertions;

namespace Streaming.Pruebas.Infraestructura;

public class PersistenciaTests
{
    private StreamingDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<StreamingDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        
        return new StreamingDbContext(options);
    }

    [Fact]
    public async Task RepositorioTransmisiones_AgregarYObtenerPorEventoId_DebeFuncionar()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var repositorio = new RepositorioTransmisiones(context);
        var eventoId = Guid.NewGuid();
        var transmision = Transmision.Crear(eventoId, PlataformaTransmision.GoogleMeet);

        // Act
        await repositorio.AgregarAsync(transmision);
        await context.SaveChangesAsync();
        
        var resultado = await repositorio.ObtenerPorEventoIdAsync(eventoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EventoId.Should().Be(eventoId);
        resultado.UrlAcceso.Should().Be(transmision.UrlAcceso);
    }

    [Fact]
    public async Task UnitOfWork_GuardarCambiosAsync_DebePersistirDatos()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var eventoId = Guid.NewGuid();
        var transmision = Transmision.Crear(eventoId, PlataformaTransmision.Zoom, "https://zoom.us/test");
        
        context.Transmisiones.Add(transmision);

        // Act
        var result = await uow.GuardarCambiosAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        var persisted = await context.Transmisiones.FirstOrDefaultAsync(t => t.EventoId == eventoId);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public void StreamingDbContext_OnModelCreating_DebeConfigurarModelo()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        // Act & Assert
        context.Model.FindEntityType(typeof(Transmision)).Should().NotBeNull();
    }
}
