using Microsoft.EntityFrameworkCore;
using Servicios.Dominio.Entidades;
using Servicios.Dominio.Repositorios;
using Servicios.Infraestructura.Persistencia;

namespace Servicios.Infraestructura.Repositorios;

public class RepositorioServicios : IRepositorioServicios
{
    private readonly ServiciosDbContext _context;

    public RepositorioServicios(ServiciosDbContext context)
    {
        _context = context;
    }

    public async Task<ServicioGlobal?> ObtenerServicioPorIdAsync(Guid id) => 
        await _context.ServiciosGlobales.FindAsync(id);

    public async Task<ServicioGlobal?> ObtenerServicioConProveedoresAsync(Guid id) =>
        await _context.ServiciosGlobales.Include(s => s.Proveedores).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<ServicioGlobal?> ObtenerServicioPorNombreAsync(string nombre) =>
        await _context.ServiciosGlobales.Include(s => s.Proveedores)
            .FirstOrDefaultAsync(s => s.Nombre.ToLower().Contains(nombre.ToLower()));

    public async Task<IEnumerable<ServicioGlobal>> ObtenerCatalogoAsync() => 
        await _context.ServiciosGlobales.Include(s => s.Proveedores).Where(s => s.Activo).ToListAsync();

    public async Task AgregarServicioAsync(ServicioGlobal servicio)
    {
        await _context.ServiciosGlobales.AddAsync(servicio);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarServicioAsync(ServicioGlobal servicio)
    {
        _context.ServiciosGlobales.Update(servicio);
        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<ReservaServicio?> ObtenerReservaPorIdAsync(Guid id) => 
        await _context.ReservasServicios.FindAsync(id);

    public async Task AgregarReservaAsync(ReservaServicio reserva)
    {
        await _context.ReservasServicios.AddAsync(reserva);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarReservaAsync(ReservaServicio reserva)
    {
        _context.ReservasServicios.Update(reserva);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReservaServicio>> ObtenerReservasPorUsuarioAsync(Guid usuarioId) => 
        await _context.ReservasServicios.Where(r => r.UsuarioId == usuarioId).ToListAsync();

    public async Task<IEnumerable<ReservaServicio>> ObtenerReservasPorOrdenEntradaAsync(Guid ordenEntradaId) =>
        await _context.ReservasServicios.Where(r => r.OrdenEntradaId == ordenEntradaId).ToListAsync();
}
