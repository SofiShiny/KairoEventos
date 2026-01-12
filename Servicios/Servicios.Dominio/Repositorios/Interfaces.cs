using Servicios.Dominio.Entidades;

namespace Servicios.Dominio.Repositorios;

public interface IRepositorioServicios
{
    Task<ServicioGlobal?> ObtenerServicioPorIdAsync(Guid id);
    Task<IEnumerable<ServicioGlobal>> ObtenerCatalogoAsync();
    Task AgregarServicioAsync(ServicioGlobal servicio);
    
    Task<ReservaServicio?> ObtenerReservaPorIdAsync(Guid id);
    Task AgregarReservaAsync(ReservaServicio reserva);
    Task ActualizarReservaAsync(ReservaServicio reserva);
    Task<IEnumerable<ReservaServicio>> ObtenerReservasPorUsuarioAsync(Guid usuarioId);
}

public interface IVerificadorEntradas
{
    Task<bool> UsuarioTieneEntradaParaEventoAsync(Guid usuarioId, Guid eventoId);
}
