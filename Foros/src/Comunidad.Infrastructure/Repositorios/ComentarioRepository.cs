using Comunidad.Domain.Entidades;
using Comunidad.Domain.Repositorios;
using Comunidad.Infrastructure.Persistencia;
using MongoDB.Driver;

namespace Comunidad.Infrastructure.Repositorios;

public class ComentarioRepository : IComentarioRepository
{
    private readonly MongoDbContext _context;

    public ComentarioRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Comentario>> ObtenerPorForoIdAsync(Guid foroId)
    {
        return await _context.Comentarios
            .Find(c => c.ForoId == foroId)
            .ToListAsync();
    }

    public async Task<Comentario?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Comentarios
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task CrearAsync(Comentario comentario)
    {
        await _context.Comentarios.InsertOneAsync(comentario);
    }

    public async Task ActualizarAsync(Comentario comentario)
    {
        await _context.Comentarios.ReplaceOneAsync(
            c => c.Id == comentario.Id,
            comentario);
    }
}
