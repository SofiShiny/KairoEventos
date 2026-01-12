using Microsoft.EntityFrameworkCore;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;

namespace Pagos.Infraestructura.Persistencia;

public class RepositorioTransacciones : IRepositorioTransacciones
{
    private readonly PagosDbContext _context;

    public RepositorioTransacciones(PagosDbContext context)
    {
        _context = context;
    }

    public async Task<Transaccion?> ObtenerPorIdAsync(Guid id) => 
        await _context.Transacciones.FindAsync(id);

    public async Task AgregarAsync(Transaccion tx)
    {
        _context.Transacciones.Add(tx);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Transaccion tx)
    {
        _context.Transacciones.Update(tx);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Transaccion>> ObtenerTodasAsync() => 
        await _context.Transacciones.ToListAsync();
}
