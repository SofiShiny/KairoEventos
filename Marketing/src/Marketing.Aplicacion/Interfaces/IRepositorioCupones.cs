using Marketing.Dominio.Entidades;

namespace Marketing.Aplicacion.Interfaces;

public interface IRepositorioCupones
{
    Task AgregarAsync(Cupon cupon);
    Task<Cupon?> ObtenerPorCodigoAsync(string codigo);
    Task<IEnumerable<Cupon>> ObtenerTodosAsync();
    Task ActualizarAsync(Cupon cupon);
}
