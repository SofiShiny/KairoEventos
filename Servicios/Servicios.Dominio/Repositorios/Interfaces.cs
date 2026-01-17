using Servicios.Dominio.Entidades;

namespace Servicios.Dominio.Repositorios;

public interface IRepositorioServicios
{
    Task<ServicioGlobal?> ObtenerServicioPorIdAsync(Guid id);
    Task<ServicioGlobal?> ObtenerServicioConProveedoresAsync(Guid id);
    Task<ServicioGlobal?> ObtenerServicioPorNombreAsync(string nombre);
    Task<IEnumerable<ServicioGlobal>> ObtenerCatalogoAsync();
    Task AgregarServicioAsync(ServicioGlobal servicio);
    Task ActualizarServicioAsync(ServicioGlobal servicio);
    Task SaveAsync();
    
    Task<ReservaServicio?> ObtenerReservaPorIdAsync(Guid id);
    Task AgregarReservaAsync(ReservaServicio reserva);
    Task ActualizarReservaAsync(ReservaServicio reserva);
    Task<IEnumerable<ReservaServicio>> ObtenerReservasPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<ReservaServicio>> ObtenerReservasPorOrdenEntradaAsync(Guid ordenEntradaId);
}

public interface IVerificadorEntradas
{
    Task<bool> UsuarioTieneEntradaParaEventoAsync(Guid usuarioId, Guid eventoId);
}
