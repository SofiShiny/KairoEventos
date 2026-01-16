using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicios.Dominio.Interfaces;

public interface IProveedorExternoService
{
    Task<Dictionary<string, bool>> ConsultarEstadoProveedoresAsync(List<string> externalIds);
    Task<IEnumerable<Servicios.Dominio.Entidades.ServicioGlobal>> ObtenerServiciosCateringAsync();
}
