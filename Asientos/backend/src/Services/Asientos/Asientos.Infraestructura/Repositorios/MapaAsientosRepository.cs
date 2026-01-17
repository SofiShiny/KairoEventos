using Asientos.Dominio.Agregados;
using Asientos.Dominio.Entidades;
using Asientos.Dominio.Repositorios;
using Microsoft.EntityFrameworkCore;
using Asientos.Infraestructura.Persistencia;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Asientos.Infraestructura.Repositorios;

public class MapaAsientosRepository : IRepositorioMapaAsientos
{
    private readonly AsientosDbContext _db;
    public MapaAsientosRepository(AsientosDbContext db) => _db = db;

    public async Task<MapaAsientos?> ObtenerPorIdAsync(Guid id, CancellationToken ct) =>
        await _db.Mapas
                 .Include(m => m.Asientos)
                 .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AgregarAsync(MapaAsientos mapa, CancellationToken ct)
    {
        await _db.Mapas.AddAsync(mapa, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(MapaAsientos mapa, CancellationToken ct)
    {
        if (_db.Entry(mapa).State == EntityState.Detached)
            _db.Mapas.Update(mapa);
            
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Guid> AgregarAsientoAsync(MapaAsientos mapa, Asiento asiento, CancellationToken ct)       
    {
        if (_db.Entry(mapa).State == EntityState.Detached)
            _db.Mapas.Attach(mapa);
        
        await _db.Asientos.AddAsync(asiento, ct);
        await _db.SaveChangesAsync(ct);
        return asiento.Id;
    }

    public async Task<Asiento?> ObtenerAsientoPorIdAsync(Guid asientoId, CancellationToken ct) =>
        await _db.Asientos.FirstOrDefaultAsync(a => a.Id == asientoId, ct);

    public async Task GuardarCambiosAsync(CancellationToken ct) => 
        await _db.SaveChangesAsync(ct);
}
