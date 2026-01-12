using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using Comunidad.Infrastructure.Persistencia;
using MongoDB.Driver;

namespace Comunidad.Infrastructure.Repositorios;

public class ForoRepository : IForoRepository
{
    private readonly MongoDbContext _context;

    public ForoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Foro?> ObtenerPorEventoIdAsync(Guid eventoId)
    {
        return await _context.Foros
            .Find(f => f.EventoId == eventoId)
            .FirstOrDefaultAsync();
    }

    public async Task CrearAsync(Foro foro)
    {
        await _context.Foros.InsertOneAsync(foro);
    }

    public async Task<bool> ExistePorEventoIdAsync(Guid eventoId)
    {
        var count = await _context.Foros
            .CountDocumentsAsync(f => f.EventoId == eventoId);
        return count > 0;
    }
}
