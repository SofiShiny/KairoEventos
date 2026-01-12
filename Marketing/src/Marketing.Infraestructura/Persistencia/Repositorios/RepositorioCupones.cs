using Microsoft.EntityFrameworkCore;
using Marketing.Aplicacion.Interfaces;
using Marketing.Dominio.Entidades;

namespace Marketing.Infraestructura.Persistencia.Repositorios;

public class RepositorioCupones : IRepositorioCupones
{
    private readonly MarketingDbContext _context;

    public RepositorioCupones(MarketingDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Cupon cupon)
    {
        await _context.Cupones.AddAsync(cupon);
        await _context.SaveChangesAsync();
    }

    public async Task<Cupon?> ObtenerPorCodigoAsync(string codigo)
    {
        return await _context.Cupones
            .FirstOrDefaultAsync(c => c.Codigo == codigo.ToUpper().Trim());
    }

    public async Task<IEnumerable<Cupon>> ObtenerTodosAsync()
    {
        return await _context.Cupones.ToListAsync();
    }

    public async Task ActualizarAsync(Cupon cupon)
    {
        _context.Cupones.Update(cupon);
        await _context.SaveChangesAsync();
    }
}
