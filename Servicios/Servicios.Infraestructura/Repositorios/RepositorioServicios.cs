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

    public async Task<IEnumerable<ServicioGlobal>> ObtenerCatalogoAsync() => 
        await _context.ServiciosGlobales.Where(s => s.Activo).ToListAsync();

    public async Task AgregarServicioAsync(ServicioGlobal servicio)
    {
        await _context.ServiciosGlobales.AddAsync(servicio);
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
}
