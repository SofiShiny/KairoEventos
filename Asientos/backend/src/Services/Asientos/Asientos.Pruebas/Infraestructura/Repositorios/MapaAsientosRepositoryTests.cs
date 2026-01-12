using Asientos.Dominio.Agregados;
using Asientos.Dominio.Entidades;
using Asientos.Infraestructura.Persistencia;
using Asientos.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Asientos.Pruebas.Infraestructura.Repositorios;

public class MapaAsientosRepositoryTests
{
    private AsientosDbContext Create()
    {
        var opts = new DbContextOptionsBuilder<AsientosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AsientosDbContext(opts);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_DebeRetornarMapa()
    {
        using var db = Create();
        var repo = new MapaAsientosRepository(db);
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        await db.Mapas.AddAsync(mapa);
        await db.SaveChangesAsync();

        var resultado = await repo.ObtenerPorIdAsync(mapa.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(mapa.Id, resultado.Id);
    }

    [Fact]
    public async Task AgregarAsync_DebeGuardarMapa()
    {
        using var db = Create();
        var repo = new MapaAsientosRepository(db);
        var mapa = MapaAsientos.Crear(Guid.NewGuid());

        await repo.AgregarAsync(mapa, CancellationToken.None);

        var dbMapa = await db.Mapas.FindAsync(mapa.Id);
        Assert.NotNull(dbMapa);
    }

    [Fact]
    public async Task ActualizarAsync_DebeActualizarMapa()
    {
        using var db = Create();
        var repo = new MapaAsientosRepository(db);
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        await db.Mapas.AddAsync(mapa);
        await db.SaveChangesAsync();

        mapa.AgregarCategoria("VIP", 200m, true);

        await repo.ActualizarAsync(mapa, CancellationToken.None);

        var dbMapa = await db.Mapas.Include(m => m.Categorias).FirstOrDefaultAsync(m => m.Id == mapa.Id);
        Assert.NotNull(dbMapa);
        Assert.Single(dbMapa.Categorias);
    }

    [Fact]
    public async Task AgregarAsientoAsync_CuandoMapaEsDetached_DebeAdjuntarYGuardar()
    {
        using var db = Create();
        var repo = new MapaAsientosRepository(db);
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("General", 100m, false);
        
        await db.Mapas.AddAsync(mapa);
        await db.SaveChangesAsync();
        
        db.Entry(mapa).State = EntityState.Detached;
        
        var asiento = mapa.AgregarAsiento(1, 1, "General");
        
        await repo.AgregarAsientoAsync(mapa, asiento, CancellationToken.None);
        
        var asientoDb = await db.Asientos.FindAsync(asiento.Id);
        Assert.NotNull(asientoDb);
        Assert.NotEqual(EntityState.Detached, db.Entry(mapa).State);
    }

    [Fact]
    public async Task ObtenerAsientoPorIdAsync_DebeRetornarAsiento()
    {
        using var db = Create();
        var repo = new MapaAsientosRepository(db);
        var mapa = MapaAsientos.Crear(Guid.NewGuid());
        mapa.AgregarCategoria("VIP", 100m, true);
        var asiento = mapa.AgregarAsiento(1, 1, "VIP");
        
        await db.Mapas.AddAsync(mapa);
        await db.Asientos.AddAsync(asiento);
        await db.SaveChangesAsync();

        var resultado = await repo.ObtenerAsientoPorIdAsync(asiento.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(asiento.Id, resultado.Id);
    }
}

