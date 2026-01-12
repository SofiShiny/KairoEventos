using Microsoft.EntityFrameworkCore;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;
using Pagos.Infraestructura.Persistencia;

namespace Pagos.Infraestructura.Repositorios;

public class CuponRepositorio : ICuponRepositorio
{
    private readonly PagosDbContext _context;

    public CuponRepositorio(PagosDbContext context)
    {
        _context = context;
    }

    public async Task<Cupon?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _context.Cupones
            .FirstOrDefaultAsync(c => c.Codigo == codigo.ToUpper(), cancellationToken);
    }

    public async Task<Cupon?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Cupones
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Cupon>> ObtenerPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        return await _context.Cupones
            .Where(c => c.EventoId == eventoId)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Cupon>> ObtenerGlobalesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cupones
            .Where(c => c.EventoId == null)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Cupon>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cupones
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(Cupon cupon, CancellationToken cancellationToken = default)
    {
        await _context.Cupones.AddAsync(cupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AgregarVariosAsync(List<Cupon> cupones, CancellationToken cancellationToken = default)
    {
        await _context.Cupones.AddRangeAsync(cupones, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Cupon cupon, CancellationToken cancellationToken = default)
    {
        _context.Cupones.Update(cupon);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _context.Cupones
            .AnyAsync(c => c.Codigo == codigo.ToUpper(), cancellationToken);
    }
}
