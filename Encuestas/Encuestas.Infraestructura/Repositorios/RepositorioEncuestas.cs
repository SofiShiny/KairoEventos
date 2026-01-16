using Microsoft.EntityFrameworkCore;
using Encuestas.Dominio.Entidades;
using Encuestas.Dominio.Repositorios;
using Encuestas.Infraestructura.Persistencia;

namespace Encuestas.Infraestructura.Repositorios;

public class RepositorioEncuestas : IRepositorioEncuestas
{
    private readonly EncuestasDbContext _context;

    public RepositorioEncuestas(EncuestasDbContext context)
    {
        _context = context;
    }

    public async Task<Encuesta?> ObtenerPorIdAsync(Guid id) =>
        await _context.Encuestas.Include(e => e.Preguntas).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Encuesta?> ObtenerPorEventoIdAsync(Guid eventoId) =>
        await _context.Encuestas.Include(e => e.Preguntas).FirstOrDefaultAsync(e => e.EventoId == eventoId);

    public async Task AgregarEncuestaAsync(Encuesta encuesta)
    {
        await _context.Encuestas.AddAsync(encuesta);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarEncuestaAsync(Encuesta encuesta)
    {
        _context.Encuestas.Update(encuesta);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UsuarioYaRespondioAsync(Guid encuestaId, Guid usuarioId) =>
        await _context.RespuestasUsuarios.AnyAsync(r => r.EncuestaId == encuestaId && r.UsuarioId == usuarioId);

    public async Task GuardarRespuestaAsync(RespuestaUsuario respuesta)
    {
        await _context.RespuestasUsuarios.AddAsync(respuesta);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasAsync(Guid encuestaId) =>
        await _context.RespuestasUsuarios
            .Include(r => r.Valores)
            .Where(r => r.EncuestaId == encuestaId)
            .OrderByDescending(r => r.Fecha)
            .ToListAsync();
}
