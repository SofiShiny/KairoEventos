using Asientos.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;

namespace Asientos.Pruebas.Infraestructura.Persistencia;

public class AsientosDbContextTests
{
    private AsientosDbContext Create()
    {
        var opts = new DbContextOptionsBuilder<AsientosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AsientosDbContext(opts);
    }

    [Fact]
    public void OnModelCreating_ConfiguraTablasYIndices()
    {
        using var db = Create();
        var modelo = db.Model;
        // Validar entidades principales
        Assert.NotNull(modelo.FindEntityType(typeof(Asientos.Dominio.Agregados.MapaAsientos)));
        Assert.NotNull(modelo.FindEntityType(typeof(Asientos.Dominio.Entidades.Asiento)));
    }
}
