using Microsoft.EntityFrameworkCore;
using Recomendaciones.Dominio.Entidades;
using Recomendaciones.Dominio.Repositorios;
using Recomendaciones.Infraestructura.Persistencia;

namespace Recomendaciones.Infraestructura.Repositorios;

public class RepositorioRecomendaciones : IRepositorioRecomendaciones
{
    private readonly RecomendacionesDbContext _context;

    public RepositorioRecomendaciones(RecomendacionesDbContext context)
    {
        _context = context;
    }

    public async Task<AfinidadUsuario?> ObtenerAfinidadAsync(Guid usuarioId, string categoria) =>
        await _context.Set<AfinidadUsuario>()
            .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.Categoria == categoria);

    public async Task<IEnumerable<AfinidadUsuario>> ObtenerAfinidadesPorUsuarioAsync(Guid usuarioId) =>
        await _context.Set<AfinidadUsuario>()
            .Where(a => a.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task AgregarAfinidadAsync(AfinidadUsuario afinidad)
    {
        await _context.Set<AfinidadUsuario>().AddAsync(afinidad);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAfinidadAsync(AfinidadUsuario afinidad)
    {
        _context.Set<AfinidadUsuario>().Update(afinidad);
        await _context.SaveChangesAsync();
    }

    public async Task<EventoProyeccion?> ObtenerEventoAsync(Guid eventoId) =>
        await _context.Set<EventoProyeccion>().FindAsync(eventoId);

    public async Task AgregarEventoAsync(EventoProyeccion evento)
    {
        await _context.Set<EventoProyeccion>().AddAsync(evento);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarEventoAsync(EventoProyeccion evento)
    {
        _context.Set<EventoProyeccion>().Update(evento);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<EventoProyeccion>> ObtenerEventosFuturosAsync() =>
        await _context.Set<EventoProyeccion>()
            .Where(e => e.Activo && e.Fecha >= DateTime.UtcNow)
            .OrderBy(e => e.Fecha)
            .ToListAsync();

    public async Task<IEnumerable<EventoProyeccion>> ObtenerEventosPorCategoriasAsync(IEnumerable<string> categorias) =>
        await _context.Set<EventoProyeccion>()
            .Where(e => e.Activo && e.Fecha >= DateTime.UtcNow && categorias.Contains(e.Categoria))
            .ToListAsync();
}
